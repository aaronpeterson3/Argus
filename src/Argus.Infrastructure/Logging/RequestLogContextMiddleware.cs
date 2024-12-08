using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Argus.Infrastructure.Logging
{
    public class RequestLogContextMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLogContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using (LogContext.PushProperty("RequestId", context.TraceIdentifier))
            using (LogContext.PushProperty("UserIp", context.Connection.RemoteIpAddress))
            using (LogContext.PushProperty("UserAgent", context.Request.Headers["User-Agent"].ToString()))
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    await _next(context);
                }
                finally
                {
                    sw.Stop();
                    LogContext.PushProperty("ResponseTimeMs", sw.ElapsedMilliseconds);
                }
            }
        }
    }
}