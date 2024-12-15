namespace Argus.Features.Users.Domain.Models
{
    public class UserState
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public UserProfile Profile { get; set; }
        public string CurrentTenantId { get; set; }
        public List<string> TenantIds { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
    }
}