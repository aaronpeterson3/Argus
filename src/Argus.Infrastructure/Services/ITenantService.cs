using Argus.Infrastructure.Models;

namespace Argus.Infrastructure.Services;

public interface ITenantService
{
    Task<TenantDto?> GetTenantAsync(Guid tenantId);
    Task<bool> HasPermissionAsync(Guid userId, Guid tenantId, string permission);
}