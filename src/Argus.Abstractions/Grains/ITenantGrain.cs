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
}

public record TenantUserInfo(
    Guid UserId,
    string Role,
    DateTime AddedAt
);