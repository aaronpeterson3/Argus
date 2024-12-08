using Dapper;

namespace Argus.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Repository for managing tenant data using Dapper
    /// </summary>
    public class TenantRepository : ITenantRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IDataEncryption _encryption;

        /// <summary>
        /// Initializes repository with database and encryption dependencies
        /// </summary>
        public TenantRepository(IDbConnectionFactory connectionFactory, IDataEncryption encryption)
        {
            _connectionFactory = connectionFactory;
            _encryption = encryption;
        }

        /// <summary>
        /// Retrieves tenant by ID with decrypted sensitive data
        /// </summary>
        /// <param name="id">Tenant ID</param>
        /// <returns>Tenant entity or null if not found</returns>
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

        /// <summary>
        /// Creates new tenant with encrypted sensitive data
        /// </summary>
        /// <param name="tenant">Tenant entity to create</param>
        /// <returns>ID of created tenant</returns>
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