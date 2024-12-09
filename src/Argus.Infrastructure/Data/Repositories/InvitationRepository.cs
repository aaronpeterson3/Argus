using Argus.Infrastructure.Data.Interfaces;
using Dapper;

namespace Argus.Infrastructure.Data.Repositories
{
    public class InvitationRepository : IInvitationRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        private const string CreateInvitationSql = @"
            INSERT INTO invitations (id, tenant_id, email, role, invited_by, token, expires_at)
            VALUES (@Id, @TenantId, @Email, @Role, @InvitedBy, @Token, @ExpiresAt)";

        private const string GetPendingInvitationSql = @"
            SELECT i.*, t.name as tenant_name, t.logo_url
            FROM invitations i
            JOIN tenants t ON i.tenant_id = t.id
            WHERE i.email = @Email
                AND i.token = @Token
                AND i.expires_at > @Now
                AND i.status = 'Pending'";

        private const string UpdateInvitationStatusSql = @"
            UPDATE invitations
            SET status = @Status,
                accepted_at = CASE WHEN @Status = 'Accepted' THEN CURRENT_TIMESTAMP ELSE NULL END
            WHERE id = @Id";

        public async Task CreateAsync(InvitationEntity invitation)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(CreateInvitationSql, invitation);
        }

        public async Task<InvitationEntity> GetPendingInvitationAsync(string email, string token)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<InvitationEntity>(
                GetPendingInvitationSql,
                new { Email = email, Token = token, Now = DateTime.UtcNow });
        }

        public async Task UpdateStatusAsync(Guid id, string status)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(
                UpdateInvitationStatusSql,
                new { Id = id, Status = status });
        }
    }
}