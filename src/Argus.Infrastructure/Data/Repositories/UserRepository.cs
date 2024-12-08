using Dapper;
using Argus.Infrastructure.Data.DTOs;
using Argus.Infrastructure.Data.Interfaces;

namespace Argus.Infrastructure.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IDataEncryption _encryption;

        private const string SelectUserById = @"
            SELECT id, email, first_name, last_name, display_name, is_active, created_at, last_login_at
            FROM users
            WHERE id = @Id";

        private const string SelectUserByEmail = @"
            SELECT id, email, password_hash, first_name, last_name, display_name, is_active, created_at, last_login_at
            FROM users
            WHERE email = @Email";

        private const string SelectUsersByTenant = @"
            SELECT u.*
            FROM users u
            INNER JOIN user_tenants ut ON u.id = ut.user_id
            WHERE ut.tenant_id = @TenantId";

        private const string InsertUser = @"
            INSERT INTO users (id, email, password_hash, first_name, last_name, display_name, is_active, created_at)
            VALUES (@Id, @Email, @PasswordHash, @FirstName, @LastName, @DisplayName, @IsActive, @CreatedAt)
            RETURNING id";

        public UserRepository(IDbConnectionFactory connectionFactory, IDataEncryption encryption)
        {
            _connectionFactory = connectionFactory;
            _encryption = encryption;
        }

        public async Task<UserDto> GetByIdAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<UserDto>(
                SelectUserById,
                new { Id = id });
        }

        public async Task<UserDto> GetByEmailAsync(string email)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<UserDto>(
                SelectUserByEmail,
                new { Email = email });
        }

        public async Task<IEnumerable<UserDto>> GetByTenantAsync(Guid tenantId)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<UserDto>(
                SelectUsersByTenant,
                new { TenantId = tenantId });
        }

        public async Task<Guid> CreateAsync(UserDto user)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                user.Id = Guid.NewGuid();
                user.CreatedAt = DateTime.UtcNow;
                user.IsActive = true;

                var id = await connection.QuerySingleAsync<Guid>(
                    InsertUser,
                    user,
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

        public async Task UpdateAsync(UserDto user)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(@"
                UPDATE users 
                SET first_name = @FirstName,
                    last_name = @LastName,
                    display_name = @DisplayName,
                    is_active = @IsActive
                WHERE id = @Id",
                user);
        }

        public async Task DeleteAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(
                "DELETE FROM users WHERE id = @Id",
                new { Id = id });
        }

        public async Task<bool> ValidateCredentialsAsync(string email, string password)
        {
            var user = await GetByEmailAsync(email);
            if (user == null) return false;

            var hashedPassword = _encryption.Encrypt(password);
            return user.PasswordHash == hashedPassword;
        }
    }
}