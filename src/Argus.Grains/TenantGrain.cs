using Orleans;
using Orleans.Runtime;
using Argus.Abstractions.Grains;

namespace Argus.Grains;

[StorageProvider(ProviderName = "tenant-store")]
public class TenantGrain : Grain, ITenantGrain
{
    private readonly IPersistentState<TenantState> _state;
    private readonly ILogger<TenantGrain> _logger;
    private readonly List<TenantUserInfo> _users = new();

    public TenantGrain(
        [PersistentState("tenant")] IPersistentState<TenantState> state,
        ILogger<TenantGrain> logger)
    {
        _state = state;
        _logger = logger;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await base.OnActivateAsync(cancellationToken);
        _logger.LogInformation("Activated tenant grain {TenantId}", this.GetPrimaryKey());
    }

    public Task<TenantState> GetStateAsync() => Task.FromResult(_state.State);

    public async Task<bool> UpdateAsync(TenantState state)
    {
        try
        {
            _state.State = state;
            await _state.WriteStateAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update tenant {TenantId}", state.Id);
            return false;
        }
    }

    public async Task<bool> AddUserAsync(Guid userId, string role)
    {
        try
        {
            if (_users.Any(u => u.UserId == userId))
                return false;

            _users.Add(new TenantUserInfo(userId, role, DateTime.UtcNow));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add user {UserId} to tenant {TenantId}", 
                userId, this.GetPrimaryKey());
            return false;
        }
    }

    public async Task<bool> RemoveUserAsync(Guid userId)
    {
        try
        {
            return _users.RemoveAll(u => u.UserId == userId) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove user {UserId} from tenant {TenantId}", 
                userId, this.GetPrimaryKey());
            return false;
        }
    }

    public async Task<bool> UpdateUserRoleAsync(Guid userId, string role)
    {
        try
        {
            var user = _users.FirstOrDefault(u => u.UserId == userId);
            if (user == null) return false;

            var index = _users.IndexOf(user);
            _users[index] = user with { Role = role };
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update role for user {UserId} in tenant {TenantId}", 
                userId, this.GetPrimaryKey());
            return false;
        }
    }

    public Task<IEnumerable<TenantUserInfo>> GetUsersAsync() => 
        Task.FromResult(_users.AsEnumerable());
}