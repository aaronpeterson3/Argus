namespace Argus.Infrastructure.MultiTenant
{
    public class TenantConfiguration
    {
        public string SchemaPrefix { get; set; } = "tenant_";
        public bool UseSchemaBasedIsolation { get; set; } = true;
        public int MaxUsersPerTenant { get; set; } = 100;
        public List<string> AvailableRoles { get; set; } = new() { "Admin", "User", "ReadOnly" };
    }

    public class TenantInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Subdomain { get; set; }
        public Dictionary<string, object> Settings { get; set; }
    }

    public class UserTenantContext
    {
        public Guid UserId { get; set; }
        public Guid TenantId { get; set; }
        public string Role { get; set; }
        public List<string> Permissions { get; set; }
    }
}