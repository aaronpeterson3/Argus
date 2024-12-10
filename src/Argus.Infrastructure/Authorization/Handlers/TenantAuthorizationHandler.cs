using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Argus.Infrastructure.Authorization.Requirements;
using Argus.Infrastructure.Services;

namespace Argus.Infrastructure.Authorization.Handlers;

public class TenantAuthorizationHandler : AuthorizationHandler<TenantPermissionRequirement>
{
    private readonly IHttpContextAccessor _httpContext;
    private readonly ITenantService _tenantService;
    private readonly ILogger<TenantAuthorizationHandler> _logger;

    public TenantAuthorizationHandler(
        IHttpContextAccessor httpContext,
        ITenantService tenantService,
        ILogger<TenantAuthorizationHandler> logger)
    {
        _httpContext = httpContext;
        _tenantService = tenantService;
        _logger = logger;
    }

    protected override async ValueTask HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TenantPermissionRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var tenantId = GetTenantIdFromRequest();
        if (!tenantId.HasValue)
        {
            _logger.LogWarning("No tenant ID found in request");
            return;
        }

        var userId = Guid.Parse(context.User.FindFirst("sub")?.Value ?? string.Empty);
        if (await _tenantService.HasPermissionAsync(userId, tenantId.Value, requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }

    private Guid? GetTenantIdFromRequest()
    {
        var context = _httpContext.HttpContext;
        if (context == null) return null;

        // Try header
        var tenantHeader = context.Request.Headers["X-Tenant-ID"].FirstOrDefault();
        if (Guid.TryParse(tenantHeader, out var headerTenantId))
        {
            return headerTenantId;
        }

        // Try endpoint metadata
        var endpoint = context.GetEndpoint();
        var routeTenantId = endpoint?.Metadata?.GetMetadata<RouteValueAttribute>()?.Value?.ToString();
        if (Guid.TryParse(routeTenantId, out var routeId))
        {
            return routeId;
        }

        // Try query string
        var queryTenantId = context.Request.Query["tenantId"].FirstOrDefault();
        return Guid.TryParse(queryTenantId, out var queryId) ? queryId : null;
    }
}