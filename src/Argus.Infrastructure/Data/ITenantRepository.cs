using Argus.Infrastructure.Models;

namespace Argus.Infrastructure.Data;

public interface ITenantRepository
{
    Task<TenantDto?> GetByIdAsync(Guid tenantId);
    Task<bool> HasPermissionAsync(Guid userId, Guid tenantId, string permission);
}