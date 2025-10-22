using System;
using System.Threading.Tasks;
using App.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace App.Common.Middleware
{
	public class GlobalExceptionMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<GlobalExceptionMiddleware> _logger;

		public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task Invoke(HttpContext context)
		{
			try
			{
				await _next(context);
			}
			catch (Exception ex)
			{
				var correlationId = context.Items.ContainsKey(CorrelationIdMiddleware.TraceIdOnlyItem)
					? context.Items[CorrelationIdMiddleware.TraceIdOnlyItem]?.ToString()
					: null;

				var errorId = Guid.NewGuid().ToString("N");

				// Push the errorId into Serilog LogContext so it's included in structured logs
				using (Serilog.Context.LogContext.PushProperty("ErrorId", errorId))
				{
					_logger.LogError(ex, "Unhandled exception {ErrorId} CorrelationId={CorrelationId} {Method} {Path}", errorId, correlationId, context.Request.Method, context.Request.Path);
				}

				// Determine environment/config so we can decide whether to expose exception messages
				var env = context.RequestServices.GetService(typeof(IHostEnvironment)) as IHostEnvironment;
				var config = context.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
				var exposeFlag = false;
				var isDev = env != null && env.IsDevelopment();
				if (config != null)
				{
					bool.TryParse(config["Diagnostics:ExposeExceptionDetails"], out exposeFlag);
				}
				var isVerbose = isDev || exposeFlag;

				// Build custom payload
				int statusCode = StatusCodes.Status500InternalServerError;
				var payload = new Dictionary<string, object?>();

				var req = context.Request;
				payload["status"] = statusCode;
				payload["error"] = ex.Message;


				var path = req.Path.Value;
				System.Diagnostics.Debug.WriteLine($"Request Path: {path}");
				if (path != null)
				{
					var sb = new System.Text.StringBuilder();
					try
					{
						sb.Append($"{req.Method} ");
					}
					catch { }
					sb.Append(path);
					sb.Append(req.QueryString.ToUriComponent());
					payload["request"] = sb.ToString();
				}

				if (ex is HttpException httpEx)
				{
					statusCode = httpEx.StatusCode;
					if (ex is ValidationException vex)
					{
						// validation errors: include the errors map
						payload["errors"] = vex.Errors;
					}
				}


				// Add standard extension-like fields
				payload["errorId"] = errorId;
				if (correlationId != null) payload["correlationId"] = correlationId;

				payload["status"] = statusCode;
				context.Response.StatusCode = statusCode;
				context.Response.ContentType = "application/problem+json";

				if (isDev || exposeFlag)
				{
					payload["exception"] = ex.ToString();
				}

				await context.Response.WriteAsJsonAsync(payload);
			}
		}
	}
}
