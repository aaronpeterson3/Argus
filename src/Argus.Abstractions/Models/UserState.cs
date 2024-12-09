namespace Argus.Abstractions.Models
{
    public class UserState
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public UserProfile Profile { get; set; }
        public bool IsActive { get; set; }
        public List<TenantAccess> TenantAccess { get; set; } = new();
    }

    public class TenantAccess
    {
        public Guid TenantId { get; set; }
        public string Role { get; set; }
    }
}