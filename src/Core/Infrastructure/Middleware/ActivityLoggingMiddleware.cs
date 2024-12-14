using Microsoft.AspNetCore.Http;

namespace Argus.Core.Infrastructure.Middleware
{
    public class ActivityLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ActivityLoggingMiddleware> _logger;
        private readonly ICurrentUserService _currentUserService;

        public ActivityLoggingMiddleware(
            RequestDelegate next,
            ILogger<ActivityLoggingMiddleware> logger,
            ICurrentUserService currentUserService)
        {
            _next = next;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var userId = _currentUserService.UserId;
            var tenantId = _currentUserService.TenantId;

            _logger.LogInformation(
                "Request {Method} {Path} started - User: {UserId}, Tenant: {TenantId}",
                context.Request.Method,
                context.Request.Path,
                userId ?? "anonymous",
                tenantId ?? "none");

            var sw = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                await _next(context);

                sw.Stop();
                _logger.LogInformation(
                    "Request {Method} {Path} completed in {ElapsedMs}ms with status {StatusCode}",
                    context.Request.Method,
                    context.Request.Path,
                    sw.ElapsedMilliseconds,
                    context.Response.StatusCode);
            }
            catch
            {
                sw.Stop();
                _logger.LogWarning(
                    "Request {Method} {Path} failed after {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    sw.ElapsedMilliseconds);
                throw;
            }
        }
    }
}