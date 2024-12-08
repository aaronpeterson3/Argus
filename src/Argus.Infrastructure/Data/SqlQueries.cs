namespace Argus.Infrastructure.Data
{
    public static class SqlQueries
    {
        public const string GetTenantById = @"
            SELECT id, name, subdomain, created_at, settings, logo_url
            FROM tenants
            WHERE id = @Id";

        public const string CreateTenant = @"
            INSERT INTO tenants (id, name, subdomain, settings, logo_url)
            VALUES (@Id, @Name, @Subdomain, @Settings, @LogoUrl)
            RETURNING id";

        public const string GetUserTenants = @"
            SELECT t.*, ut.role
            FROM tenants t
            INNER JOIN user_tenants ut ON t.id = ut.tenant_id
            WHERE ut.user_id = @UserId AND ut.status = 'Active'";

        public const string AddUserToTenant = @"
            INSERT INTO user_tenants (user_id, tenant_id, role, status, invited_by, invited_at)
            VALUES (@UserId, @TenantId, @Role, @Status, @InvitedBy, @InvitedAt)";
    }
}