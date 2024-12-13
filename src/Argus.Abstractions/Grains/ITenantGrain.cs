using Orleans;

namespace Argus.Abstractions.Grains;

public interface ITenantGrain : IGrainWithGuidKey
{
    Task<TenantState> GetStateAsync();
    Task<bool> UpdateAsync(TenantState state);
    Task<bool> AddUserAsync(Guid userId, string role);
    Task<bool> RemoveUserAsync(Guid userId);
    Task<bool> UpdateUserRoleAsync(Guid userId, string role);
    Task<IEnumerable<TenantUserInfo>> GetUsersAsync();
    Task<string> InviteUserAsync(string email, string role, Guid invitedBy);
    Task<(bool Success, string Role)> ValidateInviteAsync(string email, string inviteToken);
    Task<bool> AcceptInviteAsync(string email, string inviteToken, Guid userId);
}

public record TenantState(
    Guid Id,
    string Name,
    string Subdomain,
    string LogoUrl,
    Dictionary<string, object> Settings
)
{
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public bool IsActive { get; init; } = true;
}

public record TenantUserInfo(
    Guid UserId,
    string Role,
    DateTime AddedAt
);

public record TenantInvite(
    string Email,
    string Role,
    Guid InvitedBy,
    string Token,
    DateTime ExpiresAt
);