using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;

// Path-based policies are read from configuration into anonymous objects below.

// Ensure Logs directory exists so configured file sinks can create files if necessary
var logsDir = "Logs";
if (!System.IO.Directory.Exists(logsDir))
{
	System.IO.Directory.CreateDirectory(logsDir);
}

// Configure Serilog bootstrap logger to write to console only. File sinks are
// controlled by configuration (appsettings.json) via ReadFrom.Configuration.
Log.Logger = new LoggerConfiguration()
	.Enrich.FromLogContext()
	.WriteTo.Console()
	.CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

// Replace default logging with Serilog
builder.Host.UseSerilog((ctx, lc) =>
{
	// Read sinks and configuration from appsettings.json (including WriteTo: File)
	lc.ReadFrom.Configuration(ctx.Configuration)
	  .Enrich.FromLogContext()
	  .WriteTo.Console()
	  // Reduce noisy informational logs from authentication/token validation libraries
	  .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", Serilog.Events.LogEventLevel.Warning)
	  .MinimumLevel.Override("Microsoft.IdentityModel", Serilog.Events.LogEventLevel.Warning);
});

builder.Services.AddControllers()
	.ConfigureApiBehaviorOptions(options =>
	{
		options.InvalidModelStateResponseFactory = context =>
		{
			// Build errors dictionary: field -> string[] messages
			var errors = context.ModelState
				.Where(kv => kv.Value?.Errors?.Count > 0)
				.ToDictionary(
					kv => kv.Key ?? string.Empty,
					kv => kv.Value!.Errors.Select(e => e.ErrorMessage ?? e.Exception?.Message ?? string.Empty).ToArray()
				);

			var payload = new
			{
				status = Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest,
				error = "One or more validation errors occurred.",
				errors = errors
			};

			var result = new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(payload);
			result.ContentTypes.Add("application/problem+json");
			return result;
		};
	});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	// Add JWT Bearer support to Swagger UI so you can authenticate and call protected endpoints from the UI
	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
		Name = "Authorization",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT"
	});

	c.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
			},
			new string[] { }
		}
	});
});

// Register IHttpContextAccessor for middleware to access context if needed
builder.Services.AddHttpContextAccessor();

// Configure JWT authentication
var jwtCfg = builder.Configuration.GetSection("Jwt");
// Read PathPolicies from configuration (method-aware policy rules) into anonymous objects
var pathPolicies = jwtCfg.GetSection("PathPolicies").GetChildren()
	.Select(s => new
	{
		Pattern = s["Pattern"],
		AllowWithoutToken = s.GetSection("AllowWithoutToken").Get<string[]>(),
		RequireToken = s.GetSection("RequireToken").Get<string[]>(),
		Forbidden = s.GetSection("Forbidden").Get<string[]>()
	})
	.ToArray();
var keyString = jwtCfg.GetValue<string>("Key") ?? throw new System.Exception("JWT key not configured");
var keyBytes = System.Text.Encoding.UTF8.GetBytes(keyString);
var signingKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(keyBytes);
// Ensure the key has a stable KeyId so tokens we issue include a matching 'kid' header.
// Use SHA256 of the raw key and encode as base64url to form the KeyId.
using (var sha = System.Security.Cryptography.SHA256.Create())
{
	var hash = sha.ComputeHash(keyBytes);
	// base64url (remove padding, replace +/)
	var kid = System.Convert.ToBase64String(hash).TrimEnd('=')
		.Replace('+', '-')
		.Replace('/', '_');
	signingKey.KeyId = kid;
}

// Do not log the KeyId here to avoid emitting key identifiers in logs.

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
	options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = jwtCfg.GetValue<string>("Issuer"),
		ValidAudience = jwtCfg.GetValue<string>("Audience"),
		IssuerSigningKey = signingKey
	};

	// If a token does not include a 'kid' (older tokens), provide a resolver that returns
	// the configured symmetric signing key. This allows validating tokens issued before
	// we started populating KeyId while still preferring kid-based resolution when present.
	options.TokenValidationParameters.IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) =>
	{
		// Return the single signing key we configured.
		return new[] { signingKey };
	};

	// The tokens use a custom "user_id" claim for the subject and a "roles" array for roles.
	// Map them to the framework claim types so [Authorize] and role-based checks work.
	options.TokenValidationParameters.NameClaimType = "user_id";
	options.TokenValidationParameters.RoleClaimType = "roles";

	// Skip JwtBearer processing for paths/methods configured in PathPolicies that allow unauthenticated access.
	options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
	{
		OnMessageReceived = context =>
		{
			try
			{
				var req = context.Request;
				var path = req.Path.Value ?? string.Empty;
				var method = req.Method;
				foreach (var policy in pathPolicies)
				{
					if (string.IsNullOrWhiteSpace(policy.Pattern)) continue;
					var pattern = policy.Pattern!;
					var matched = false;
					if (pattern.EndsWith("/*"))
					{
						var prefix = pattern.Substring(0, pattern.Length - 1);
						matched = path.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase);
					}
					else
					{
						matched = path.Equals(pattern, System.StringComparison.OrdinalIgnoreCase);
					}

					if (!matched) continue;

					if (policy.AllowWithoutToken != null)
					{
						foreach (var m in policy.AllowWithoutToken)
						{
							if (string.Equals(m, method, System.StringComparison.OrdinalIgnoreCase))
							{
								context.NoResult();
								return System.Threading.Tasks.Task.CompletedTask;
							}
						}
					}
				}
			}
			catch { }
			return System.Threading.Tasks.Task.CompletedTask;
		}
	};
});

// PathPolicies enforcement middleware is registered after the app is built below.

// Require authentication by default for all endpoints; allow anonymous where explicitly opted out
builder.Services.AddAuthorization(options =>
{
	options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
		.RequireAuthenticatedUser()
		.Build();
});

// App services
builder.Services.AddScoped<App.Features.Auth.Services.ITokenService, App.Features.Auth.Services.TokenService>();
var configuration = builder.Configuration;
builder.Services.AddDbContext<App.Data.AppDbContext>(opt => opt.UseNpgsql(configuration.GetConnectionString("Postgres")));
builder.Services.AddScoped<App.Features.Zaps.Repos.IZapsRepo, App.Features.Zaps.Repos.EfZapsRepo>();
builder.Services.AddScoped<App.Features.CarDict.Repos.ICarDictRepo, App.Features.CarDict.Repos.MySqlCarDictRepo>();
// Accounts: prefer EF-backed repo that maps to existing 'accounts' table. No migrations will be run.
builder.Services.AddScoped<App.Features.Accounts.IAccountsRepo, App.Features.Accounts.EfAccountsRepo>();
// Prefer EF-backed roles and region repositories that map to existing tables. No migrations will be run.
builder.Services.AddScoped<App.Features.Roles.IRolesRepo, App.Features.Roles.EfRolesRepo>();
builder.Services.AddScoped<App.Features.Dict.Region.IRegionDictRepo, App.Features.Dict.Region.EfRegionRepo>();

// Force the app to bind to IPv4 loopback on port 5002 to avoid macOS system services on 5000
// This can still be overridden by the ASPNETCORE_URLS env var or command-line args.
builder.WebHost.UseUrls("http://127.0.0.1:5002");

var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
app.UseSwagger();
app.UseSwaggerUI();
// }

// Important: routing must be configured before authentication/authorization middleware
app.UseRouting();

// Correlation ID should be set very early so logs and exception middleware can read it
app.UseMiddleware<App.Common.Middleware.CorrelationIdMiddleware>();

// Global exception handling middleware - must be early in pipeline
app.UseMiddleware<App.Common.Middleware.GlobalExceptionMiddleware>();

// PathPolicies enforcement middleware (Forbidden methods and RequireToken checks)
app.Use(async (context, next) =>
{
	try
	{
		var path = context.Request.Path.Value ?? string.Empty;
		var method = context.Request.Method;
		foreach (var policy in pathPolicies)
		{
			if (string.IsNullOrWhiteSpace(policy.Pattern)) continue;
			var pattern = policy.Pattern;
			var matched = false;
			if (pattern!.EndsWith("/*"))
			{
				var prefix = pattern.Substring(0, pattern.Length - 1);
				matched = path.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase);
			}
			else
			{
				matched = path.Equals(pattern, System.StringComparison.OrdinalIgnoreCase);
			}

			if (!matched) continue;

			// Forbidden methods -> 405
			if (policy.Forbidden != null)
			{
				foreach (var m in policy.Forbidden)
				{
					if (string.Equals(m, method, System.StringComparison.OrdinalIgnoreCase))
					{
						context.Response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status405MethodNotAllowed;
						await context.Response.WriteAsync(string.Empty);
						return;
					}
				}
			}

			// Require token presence -> 401 if Authorization header is missing
			if (policy.RequireToken != null)
			{
				foreach (var m in policy.RequireToken)
				{
					if (string.Equals(m, method, System.StringComparison.OrdinalIgnoreCase))
					{
						if (!context.Request.Headers.TryGetValue("Authorization", out var auth) || string.IsNullOrWhiteSpace(auth) || !auth.ToString().StartsWith("Bearer ", System.StringComparison.OrdinalIgnoreCase))
						{
							context.Response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized;
							var pd = new Microsoft.AspNetCore.Mvc.ProblemDetails
							{
								Title = "Authentication required",
								Status = Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized,
								Instance = context.Request.Path
							};
							context.Response.ContentType = "application/problem+json";
							await context.Response.WriteAsJsonAsync(pd);
							return;
						}
					}
				}
			}
		}
	}
	catch { }
	await next();
});

app.UseAuthentication();
app.UseAuthorization();

// After authentication runs, push a lightweight, non-PII user identifier into the
// Serilog LogContext so all subsequent logs for the request include it.
app.Use(async (context, next) =>
{
	try
	{
		// Our tokens map the name claim type to "user_id" earlier in the Jwt options.
		var userId = context.User?.FindFirst("user_id")?.Value
					 ?? context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

		if (!string.IsNullOrWhiteSpace(userId))
		{
			using (Serilog.Context.LogContext.PushProperty("UserId", userId))
			{
				await next();
				return;
			}
		}
	}
	catch { /* don't fail the pipeline for logging enrichment */ }

	await next();
});

// Global exception logging middleware (captures unhandled exceptions to a file)
app.Use(async (context, next) =>
{
	try
	{
		await next();
	}
	catch (System.Exception ex)
	{
		try
		{
			app.Logger.LogError(ex, "Unhandled exception while processing request");
		}
		catch { }
		throw;
	}
});

app.UseStaticFiles();
app.MapControllers();

app.Run();
