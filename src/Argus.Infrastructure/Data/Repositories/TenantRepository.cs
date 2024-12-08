using Dapper;
using Argus.Infrastructure.Data.DTOs;
using Argus.Infrastructure.Data.Interfaces;

namespace Argus.Infrastructure.Data.Repositories
{
    public class TenantRepository : ITenantRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IDataEncryption _encryption;

        private const string SelectTenantById = @"
            SELECT id, name, subdomain, logo_url, created_at, settings
            FROM tenants
            WHERE id = @Id";

        private const string SelectTenantBySubdomain = @"
            SELECT id, name, subdomain, logo_url, created_at, settings
            FROM tenants
            WHERE subdomain = @Subdomain";

        private const string InsertTenant = @"
            INSERT INTO tenants (id, name, subdomain, logo_url, created_at, settings)
            VALUES (@Id, @Name, @Subdomain, @LogoUrl, @CreatedAt, @Settings::jsonb)
            RETURNING id";

        public TenantRepository(IDbConnectionFactory connectionFactory, IDataEncryption encryption)
        {
            _connectionFactory = connectionFactory;
            _encryption = encryption;
        }

        public async Task<TenantDto> GetByIdAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<TenantDto>(
                SelectTenantById,
                new { Id = id });
        }

        public async Task<TenantDto> GetBySubdomainAsync(string subdomain)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<TenantDto>(
                SelectTenantBySubdomain,
                new { Subdomain = subdomain });
        }

        public async Task<IEnumerable<TenantDto>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<TenantDto>("SELECT * FROM tenants");
        }

        public async Task<Guid> CreateAsync(TenantDto tenant)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                tenant.Id = Guid.NewGuid();
                tenant.CreatedAt = DateTime.UtcNow;

                var id = await connection.QuerySingleAsync<Guid>(
                    InsertTenant,
                    tenant,
                    transaction);

                await transaction.CommitAsync();
                return id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateAsync(TenantDto tenant)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(@"
                UPDATE tenants 
                SET name = @Name,
                    subdomain = @Subdomain,
                    logo_url = @LogoUrl,
                    settings = @Settings::jsonb
                WHERE id = @Id",
                tenant);
        }

        public async Task DeleteAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(
                "DELETE FROM tenants WHERE id = @Id",
                new { Id = id });
        }
    }
}