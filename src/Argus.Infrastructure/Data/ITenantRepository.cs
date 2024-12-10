namespace Argus.Infrastructure.Data;

public interface ITenantRepository
{
    Task<bool> HasPermissionAsync(Guid userId, Guid tenantId, string permission);
}