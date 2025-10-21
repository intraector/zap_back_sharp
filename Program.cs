using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure JWT authentication
var jwtCfg = builder.Configuration.GetSection("Jwt");
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

// Log the KeyId (safe) so operators can confirm which kid the server expects.
builder.Logging.AddConsole();
var tempKid = signingKey.KeyId ?? "(none)";
System.Console.WriteLine($"JWT signing KeyId: {tempKid}");

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
});

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

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

// Important: routing must be configured before authentication/authorization middleware
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

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
