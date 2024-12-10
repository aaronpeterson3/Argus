using Dapper;

namespace Argus.Infrastructure.Data;

public class TenantRepository : ITenantRepository
{
    private readonly IDbConnectionFactory _db;

    public TenantRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<bool> HasPermissionAsync(Guid userId, Guid tenantId, string permission)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT COUNT(1) FROM user_tenant_permissions utp
            JOIN permissions p ON utp.permission_id = p.id
            WHERE utp.user_id = @userId 
            AND utp.tenant_id = @tenantId 
            AND p.name = @permission";

        var count = await conn.ExecuteScalarAsync<int>(sql, new { userId, tenantId, permission });
        return count > 0;
    }
}