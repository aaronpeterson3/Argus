namespace Argus.Abstractions.Models
{
    public record TenantInvite(
        string Email,
        string Role,
        Guid InvitedBy,
        string Token,
        DateTime ExpiresAt
    );
}