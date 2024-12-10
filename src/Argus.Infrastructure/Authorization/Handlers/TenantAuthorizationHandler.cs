using Argus.Infrastructure.Authorization.Requirements;
using Argus.Infrastructure.Authorization.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Argus.Infrastructure.Authorization.Handlers;

public class TenantAuthorizationHandler : 
    AuthorizationHandler<TenantPermissionRequirement>, IAuthorizationHandler
{
    private readonly IHttpContextAccessor _httpContext;
    private readonly ITenantPermissionService _permissionService;
    private readonly ILogger<TenantAuthorizationHandler> _logger;

    public TenantAuthorizationHandler(
        IHttpContextAccessor httpContext,
        ITenantPermissionService permissionService,
        ILogger<TenantAuthorizationHandler> logger)
    {
        _httpContext = httpContext;
        _permissionService = permissionService;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TenantPermissionRequirement requirement)
    {
        var user = context.User;
        if (!user?.Identity?.IsAuthenticated ?? false)
        {
            return;
        }

        var tenantId = GetTenantIdFromRequest();
        if (!tenantId.HasValue)
        {
            _logger.LogWarning("No tenant ID found in request");
            return;
        }

        var userId = Guid.Parse(user.FindFirst("sub")?.Value);
        if (await _permissionService.HasPermissionAsync(userId, tenantId.Value, requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }

    private Guid? GetTenantIdFromRequest()
    {
        // First try header
        var tenantHeader = _httpContext.HttpContext?.Request.Headers["X-Tenant-ID"].FirstOrDefault();
        if (Guid.TryParse(tenantHeader, out var headerTenantId))
        {
            return headerTenantId;
        }

        // Then try route data
        var routeTenantId = _httpContext.HttpContext?.Request.RouteValues["tenantId"]?.ToString();
        if (Guid.TryParse(routeTenantId, out var routeId))
        {
            return routeId;
        }

        // Finally try query string
        var queryTenantId = _httpContext.HttpContext?.Request.Query["tenantId"].FirstOrDefault();
        return Guid.TryParse(queryTenantId, out var queryId) ? queryId : null;
    }
}