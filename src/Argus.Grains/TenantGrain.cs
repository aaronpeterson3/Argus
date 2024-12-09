using Argus.Abstractions.Grains;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using System.Security.Cryptography;
using TenantGrains = Argus.Abstractions.Grains;

namespace Argus.Grains;

[StorageProvider(ProviderName = "tenant-store")]
public class TenantGrain : Grain, ITenantGrain
{
    private readonly IPersistentState<TenantGrains.TenantState> _state;
    private readonly IPersistentState<Dictionary<string, TenantGrains.TenantInvite>> _invites;
    private readonly ILogger<TenantGrain> _logger;
    private readonly List<TenantGrains.TenantUserInfo> _users = new();

    public TenantGrain(
        [PersistentState("tenant")] IPersistentState<TenantGrains.TenantState> state,
        [PersistentState("invites")] IPersistentState<Dictionary<string, TenantGrains.TenantInvite>> invites,
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
            _invites.State = new Dictionary<string, TenantGrains.TenantInvite>();
    }

    public Task<TenantGrains.TenantState> GetStateAsync() => Task.FromResult(_state.State);

    public async Task<bool> UpdateAsync(TenantGrains.TenantState state)
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

    public Task<bool> AddUserAsync(Guid userId, string role)
    {
        try
        {
            if (_users.Any(u => u.UserId == userId))
                return Task.FromResult(false);

            _users.Add(new TenantGrains.TenantUserInfo(userId, role, DateTime.UtcNow));
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add user {UserId} to tenant {TenantId}", 
                userId, this.GetPrimaryKey());
            return Task.FromResult(false);
        }
    }

    public Task<bool> RemoveUserAsync(Guid userId)
    {
        try
        {
            return Task.FromResult(_users.RemoveAll(u => u.UserId == userId) > 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove user {UserId} from tenant {TenantId}", 
                userId, this.GetPrimaryKey());
            return Task.FromResult(false);
        }
    }

    public Task<bool> UpdateUserRoleAsync(Guid userId, string role)
    {
        try
        {
            var user = _users.FirstOrDefault(u => u.UserId == userId);
            if (user == null) return Task.FromResult(false);

            var index = _users.IndexOf(user);
            _users[index] = user with { Role = role };
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update role for user {UserId} in tenant {TenantId}", 
                userId, this.GetPrimaryKey());
            return Task.FromResult(false);
        }
    }

    public Task<IEnumerable<TenantGrains.TenantUserInfo>> GetUsersAsync() => 
        Task.FromResult(_users.AsEnumerable());

    public async Task<string> InviteUserAsync(string email, string role, Guid invitedBy)
    {
        try
        {
            var token = GenerateInviteToken();
            var invite = new TenantGrains.TenantInvite(
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