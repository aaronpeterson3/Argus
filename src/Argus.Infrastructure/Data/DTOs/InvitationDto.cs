namespace Argus.Infrastructure.Data.DTOs
{
    public class InvitationDto
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string InvitedBy { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Status { get; set; }
        public DateTime? AcceptedAt { get; set; }
        
        // Join fields from tenant
        public string TenantName { get; set; }
        public string LogoUrl { get; set; }
    }
}