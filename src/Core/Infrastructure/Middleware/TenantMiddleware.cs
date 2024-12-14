using Microsoft.AspNetCore.Http;

namespace Argus.Core.Infrastructure.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ICurrentUserService currentUserService)
        {
            var tenantId = currentUserService.TenantId;
            
            if (string.IsNullOrEmpty(tenantId) && RequiresTenant(context))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { error = "Tenant ID is required for this operation" });
                return;
            }

            await _next(context);
        }

        private bool RequiresTenant(HttpContext context)
        {
            // Skip tenant check for authentication endpoints
            if (context.Request.Path.StartsWithSegments("/api/auth"))
                return false;

            // Skip tenant check for tenant creation
            if (context.Request.Path.StartsWithSegments("/api/tenants") && 
                context.Request.Method == HttpMethods.Post)
                return false;

            return true;
        }
    }
}