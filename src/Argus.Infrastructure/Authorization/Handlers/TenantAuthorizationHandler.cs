using Argus.Infrastructure.Authorization.Requirements;
using Argus.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Argus.Infrastructure.Authorization.Handlers;

public class TenantAuthorizationHandler : AuthorizationHandler<TenantPermissionRequirement>
{
    private const string TenantHeader = "X-Tenant-ID";
    private const string TenantQueryParam = "tenantId";
    private const string SubClaim = "sub";

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

    protected override async Task HandleRequirementAsync(
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

        var tenant = await _tenantService.GetTenantAsync(tenantId.Value);
        if (tenant == null || !tenant.IsActive)
        {
            _logger.LogWarning("Tenant {TenantId} not found or inactive", tenantId.Value);
            return;
        }

        var userId = Guid.Parse(context.User.FindFirst(SubClaim)?.Value ?? string.Empty);
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
        var tenantHeader = context.Request.Headers[TenantHeader].FirstOrDefault();
        if (Guid.TryParse(tenantHeader, out var headerTenantId))
        {
            return headerTenantId;
        }

        // Try route values
        var endpoint = context.GetEndpoint();
        if (endpoint != null)
        {
            var routeValues = context.GetRouteData()?.Values;
            if (routeValues != null && routeValues.TryGetValue(TenantQueryParam, out var routeValue))
            {
                if (Guid.TryParse(routeValue?.ToString(), out var routeId))
                {
                    return routeId;
                }
            }
        }

        // Try query string
        var queryTenantId = context.Request.Query[TenantQueryParam].FirstOrDefault();
        return Guid.TryParse(queryTenantId, out var queryId) ? queryId : null;
    }
}