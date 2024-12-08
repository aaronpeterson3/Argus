using Dapper;

namespace Argus.Infrastructure.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IDataEncryption _encryption;

        private const string GetUserByEmailSql = @"
            SELECT id, email, password_hash, first_name, last_name, display_name, is_active
            FROM users
            WHERE email = @Email";

        private const string CreateUserSql = @"
            INSERT INTO users (id, email, password_hash, first_name, last_name, display_name, is_active)
            VALUES (@Id, @Email, @PasswordHash, @FirstName, @LastName, @DisplayName, @IsActive)
            RETURNING id";

        private const string UpdateUserSql = @"
            UPDATE users
            SET password_hash = @PasswordHash,
                first_name = @FirstName,
                last_name = @LastName,
                display_name = @DisplayName,
                is_active = @IsActive
            WHERE id = @Id";

        public async Task<UserEntity> GetByEmailAsync(string email)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<UserEntity>(
                GetUserByEmailSql,
                new { Email = email });
        }

        public async Task<Guid> CreateAsync(UserEntity user)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                var id = await connection.QuerySingleAsync<Guid>(
                    CreateUserSql,
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

        public async Task UpdateAsync(UserEntity user)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(UpdateUserSql, user);
        }
    }
}