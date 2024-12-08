using Dapper;

namespace Argus.Infrastructure.Data.Repositories
{
    public class TenantRepository : ITenantRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IDataEncryption _encryption;

        public TenantRepository(IDbConnectionFactory connectionFactory, IDataEncryption encryption)
        {
            _connectionFactory = connectionFactory;
            _encryption = encryption;
        }

        public async Task<TenantEntity> GetByIdAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var tenant = await connection.QuerySingleOrDefaultAsync<TenantEntity>(
                SqlQueries.GetTenantById,
                new { Id = id });

            if (tenant != null)
            {
                DecryptSensitiveData(tenant);
            }

            return tenant;
        }

        public async Task<Guid> CreateAsync(TenantEntity tenant)
        {
            EncryptSensitiveData(tenant);

            using var connection = _connectionFactory.CreateConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                var id = await connection.QuerySingleAsync<Guid>(
                    SqlQueries.CreateTenant,
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

        private void EncryptSensitiveData(TenantEntity tenant)
        {
            if (!string.IsNullOrEmpty(tenant.TaxId))
            {
                tenant.TaxId = _encryption.Encrypt(tenant.TaxId);
            }
        }

        private void DecryptSensitiveData(TenantEntity tenant)
        {
            if (!string.IsNullOrEmpty(tenant.TaxId))
            {
                tenant.TaxId = _encryption.Decrypt(tenant.TaxId);
            }
        }
    }
}