using System.Collections.Immutable;

namespace Argus.Features.Tenants.Domain.Models
{
    public class TenantState
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string OwnerId { get; set; }
        public ImmutableList<TenantUserInfo> Users { get; set; }
        public ImmutableList<TenantInvite> PendingInvites { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class TenantUserInfo
    {
        public string UserId { get; set; }
        public string Role { get; set; }
        public DateTime JoinedAt { get; set; }
    }

    public class TenantInvite
    {
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Token { get; set; }
    }
}