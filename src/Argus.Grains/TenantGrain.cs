using Orleans;
using Orleans.Runtime;
using Argus.Abstractions.Grains;
using System.Security.Cryptography;

namespace Argus.Grains;

[StorageProvider(ProviderName = "tenant-store")]
public class TenantGrain : Grain, ITenantGrain
{
    private readonly IPersistentState<TenantState> _state;
    private readonly IPersistentState<Dictionary<string, TenantInvite>> _invites;
    private readonly ILogger<TenantGrain> _logger;
    private readonly List<TenantUserInfo> _users = new();

    public TenantGrain(
        [PersistentState("tenant")] IPersistentState<TenantState> state,
        [PersistentState("invites")] IPersistentState<Dictionary<string, TenantInvite>> invites,
        ILogger<TenantGrain> logger)
    {
        _state = state;
        _invites = invites;
        _logger = logger;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await base.OnActivateAsync(cancellationToken);
        _logger.LogInformation("Activated tenant grain {TenantId}", this.GetPrimaryKey());
        
        if (_invites.State == null)
            _invites.State = new Dictionary<string, TenantInvite>();
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

    public async Task<string> InviteUserAsync(string email, string role, Guid invitedBy)
    {
        try
        {
            var token = GenerateInviteToken();
            var invite = new TenantInvite(
                email,
                role,
                invitedBy,
                token,
                DateTime.UtcNow.AddDays(7)
            );

            _invites.State[token] = invite;
            await _invites.WriteStateAsync();

            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create invite for {Email} in tenant {TenantId}", 
                email, this.GetPrimaryKey());
            throw;
        }
    }

    public async Task<bool> AcceptInviteAsync(string email, string inviteToken)
    {
        try
        {
            if (!_invites.State.TryGetValue(inviteToken, out var invite))
                return false;

            if (invite.Email != email || invite.ExpiresAt < DateTime.UtcNow)
                return false;

            _invites.State.Remove(inviteToken);
            await _invites.WriteStateAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to accept invite for {Email} in tenant {TenantId}", 
                email, this.GetPrimaryKey());
            return false;
        }
    }

    private string GenerateInviteToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}