namespace Argus.Infrastructure.Data.DTOs
{
    /// <summary>
    /// Data Transfer Object for Tenant information
    /// </summary>
    public class TenantDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Subdomain { get; set; }
        public string LogoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public Dictionary<string, object> Settings { get; set; }
    }
}