using Dapper;
using Argus.Infrastructure.Models;

namespace Argus.Infrastructure.Data;

public class TenantRepository : ITenantRepository
{
    private readonly IDbConnectionFactory _db;

    public TenantRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<TenantDto?> GetByIdAsync(Guid tenantId)
    {
        using var conn = await _db.CreateConnectionAsync();
        const string sql = @"
            SELECT t.id, t.name, t.is_active, array_agg(p.name) as permissions
            FROM tenants t
            LEFT JOIN tenant_permissions tp ON t.id = tp.tenant_id
            LEFT JOIN permissions p ON tp.permission_id = p.id
            WHERE t.id = @tenantId
            GROUP BY t.id, t.name, t.is_active";

        return await conn.QuerySingleOrDefaultAsync<TenantDto>(sql, new { tenantId });
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