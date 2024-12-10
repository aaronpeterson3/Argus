using Argus.Infrastructure.Data;
using Dapper;
using System;
using System.Threading.Tasks;

namespace Argus.Infrastructure.Services
{
    public interface ITenantLogoStorage
    {
        Task<byte[]?> GetLogoAsync(Guid tenantId);
        Task SaveLogoAsync(Guid tenantId, byte[] logoData);
    }

    public class TenantLogoStorage : ITenantLogoStorage
    {
        private readonly IDbConnectionFactory _db;

        public TenantLogoStorage(IDbConnectionFactory db)
        {
            _db = db;
        }

        public async Task<byte[]?> GetLogoAsync(Guid tenantId)
        {
            using var conn = await _db.CreateConnectionAsync();
            const string sql = "SELECT logo_data FROM tenant_logos WHERE tenant_id = @tenantId";
            return await conn.QuerySingleOrDefaultAsync<byte[]>(sql, new { tenantId });
        }

        public async Task SaveLogoAsync(Guid tenantId, byte[] logoData)
        {
            using var conn = await _db.CreateConnectionAsync();
            const string sql = @"
                INSERT INTO tenant_logos (tenant_id, logo_data)
                VALUES (@tenantId, @logoData)
                ON CONFLICT (tenant_id)
                DO UPDATE SET logo_data = @logoData";

            await conn.ExecuteAsync(sql, new { tenantId, logoData });
        }
    }
}