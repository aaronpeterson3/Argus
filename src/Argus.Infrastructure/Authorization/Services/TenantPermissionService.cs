using Argus.Infrastructure.Authorization.Requirements;
using Argus.Infrastructure.Data;

namespace Argus.Infrastructure.Authorization.Services;

public interface ITenantPermissionService
{
    Task<bool> HasPermissionAsync(Guid userId, Guid tenantId, string permission);
    Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId, Guid tenantId);
}

public class TenantPermissionService : ITenantPermissionService
{
    private readonly Dictionary<string, string[]> _rolePermissions = new()
    {
        ["Owner"] = [
            TenantPermissions.ViewData,
            TenantPermissions.EditData,
            TenantPermissions.DeleteData,
            TenantPermissions.ManageUsers,
            TenantPermissions.ViewReports,
            TenantPermissions.ManageSettings
        ],
        ["Admin"] = [
            TenantPermissions.ViewData,
            TenantPermissions.EditData,
            TenantPermissions.ViewReports,
            TenantPermissions.ManageSettings
        ],
        ["User"] = [
            TenantPermissions.ViewData,
            TenantPermissions.ViewReports
        ]
    };

    private readonly ITenantRepository _tenantRepository;

    public TenantPermissionService(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<bool> HasPermissionAsync(Guid userId, Guid tenantId, string permission)
    {
        var userRole = await GetUserRoleAsync(userId, tenantId);
        if (userRole == null) return false;

        return _rolePermissions.TryGetValue(userRole, out var permissions) &&
               permissions.Contains(permission);
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId, Guid tenantId)
    {
        var userRole = await GetUserRoleAsync(userId, tenantId);
        if (userRole == null) return Array.Empty<string>();

        return _rolePermissions.TryGetValue(userRole, out var permissions) ?
            permissions : Array.Empty<string>();
    }

    private async Task<string> GetUserRoleAsync(Guid userId, Guid tenantId)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant == null) return null;

        // Get user's role from tenant membership
        // This would come from your actual data store
        return "User";
    }
}