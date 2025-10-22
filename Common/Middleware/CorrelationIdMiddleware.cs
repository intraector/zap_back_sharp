using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace App.Common.Middleware
{
	// W3C traceparent header handler. Exposes TraceParent and TraceId in HttpContext.Items
	public class CorrelationIdMiddleware
	{
		private readonly RequestDelegate _next;
		public const string TraceParentHeader = "traceparent";
		public const string TraceIdItem = "TraceParent"; // full traceparent
		public const string TraceIdOnlyItem = "TraceId"; // trace-id portion

		public CorrelationIdMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		private static string CreateTraceParent()
		{
			// version 00, trace-id (16 bytes hex), span-id (8 bytes hex), flags 01
			var traceId = Guid.NewGuid().ToString("N"); // 32 hex chars
			var spanId = Guid.NewGuid().ToString("N").Substring(0, 16);
			return $"00-{traceId}-{spanId}-01";
		}

		public async Task Invoke(HttpContext context)
		{
			string traceParent = string.Empty;
			if (context.Request.Headers.ContainsKey(TraceParentHeader))
			{
				traceParent = context.Request.Headers[TraceParentHeader].ToString();
			}

			if (string.IsNullOrWhiteSpace(traceParent))
			{
				traceParent = CreateTraceParent();
			}

			context.Items[TraceIdItem] = traceParent;
			// extract trace-id (between first '-' and second '-')
			var parts = traceParent.Split('-');
			if (parts.Length >= 2)
			{
				context.Items[TraceIdOnlyItem] = parts[1];
			}

			// ensure response has traceparent header
			context.Response.OnStarting(() =>
			{
				context.Response.Headers[TraceParentHeader] = traceParent;
				// Also expose the trace-id portion as a simple header for clients/proxies
				try
				{
					var traceIdOnly = context.Items.ContainsKey(TraceIdOnlyItem) ? context.Items[TraceIdOnlyItem]?.ToString() : null;
					if (!string.IsNullOrEmpty(traceIdOnly))
					{
						context.Response.Headers["X-Trace-Id"] = traceIdOnly;
					}
				}
				catch { }
				return Task.CompletedTask;
			});

			// Push TraceId into Serilog LogContext so logs are enriched automatically
			var traceIdForLog = context.Items.ContainsKey(TraceIdOnlyItem) ? context.Items[TraceIdOnlyItem]?.ToString() : null;
			if (!string.IsNullOrEmpty(traceIdForLog))
			{
				using (LogContext.PushProperty("TraceId", traceIdForLog))
				{
					await _next(context);
				}
			}
			else
			{
				await _next(context);
			}
		}
	}
}
