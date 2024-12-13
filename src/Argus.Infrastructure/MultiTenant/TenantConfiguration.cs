namespace Argus.Infrastructure.MultiTenant
{
    public class TenantConfiguration
    {
        public string SchemaPrefix { get; set; } = "tenant_";
        public bool UseSchemaBasedIsolation { get; set; } = true;
        public int MaxUsersPerTenant { get; set; } = 100;
        public List<string> AvailableRoles { get; set; } = new() { "Admin", "User", "ReadOnly" };
    }
}