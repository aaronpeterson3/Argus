using Argus.Infrastructure.Data.DTOs;
using Argus.Infrastructure.Data.Interfaces;
using Dapper;

namespace Argus.Infrastructure.Data.Repositories
{
    public class InvitationRepository : IInvitationRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        private const string CreateInvitationSql = @"
            INSERT INTO invitations (id, tenant_id, email, role, invited_by, token, expires_at, status)
            VALUES (@Id, @TenantId, @Email, @Role, @InvitedBy, @Token, @ExpiresAt, 'Pending')";

        private const string GetPendingInvitationSql = @"
            SELECT 
                i.id,
                i.tenant_id,
                i.email,
                i.role,
                i.invited_by as InvitedBy,
                i.token,
                i.expires_at as ExpiresAt,
                i.status,
                i.accepted_at as AcceptedAt,
                t.name as TenantName,
                t.logo_url as LogoUrl
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

        public InvitationRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task CreateAsync(InvitationDto invitation)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(CreateInvitationSql, invitation);
        }

        public async Task<InvitationDto> GetPendingInvitationAsync(string email, string token)
        {
            using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<InvitationDto>(
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