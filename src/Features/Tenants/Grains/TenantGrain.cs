using Orleans;
using System.Collections.Immutable;
using Argus.Features.Tenants.Domain.Models;
using Argus.Core.Common;
using Argus.Features.Authentication.Domain.Services;

namespace Argus.Features.Tenants.Grains
{
    public interface ITenantGrain : IGrainWithStringKey
    {
        Task<TenantState> GetStateAsync();
        Task<Result<string>> CreateAsync(string name, string ownerId);
        Task<Result<Unit>> UpdateProfileAsync(string name);
        Task<Result<string>> InviteUserAsync(string email, string role);
        Task<Result<Unit>> AcceptInviteAsync(string token, string userId);
    }

    public class TenantGrain : Grain, ITenantGrain
    {
        private TenantState _state;
        private readonly IPersistentState<TenantState> _storage;
        private readonly IEmailService _emailService;

        public TenantGrain(
            [PersistentState("tenant")] IPersistentState<TenantState> storage,
            IEmailService emailService)
        {
            _storage = storage;
            _emailService = emailService;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await base.OnActivateAsync(cancellationToken);
            _state = _storage.State;
        }

        public Task<TenantState> GetStateAsync()
        {
            return Task.FromResult(_state);
        }

        public async Task<Result<string>> CreateAsync(string name, string ownerId)
        {
            if (_state != null)
                return Result<string>.Failure("Tenant already exists");

            _state = new TenantState
            {
                Id = this.GetPrimaryKeyString(),
                Name = name,
                OwnerId = ownerId,
                Users = new List<TenantUserInfo>
                {
                    new TenantUserInfo
                    {
                        UserId = ownerId,
                        Role = "Owner",
                        JoinedAt = DateTime.UtcNow
                    }
                }.ToImmutableList(),
                PendingInvites = ImmutableList<TenantInvite>.Empty,
                CreatedAt = DateTime.UtcNow
            };

            await _storage.WriteStateAsync();
            return Result<string>.Success(_state.Id);
        }

        public async Task<Result<Unit>> UpdateProfileAsync(string name)
        {
            if (_state == null)
                return Result<Unit>.Failure("Tenant not found");

            _state.Name = name;
            _state.UpdatedAt = DateTime.UtcNow;

            await _storage.WriteStateAsync();
            return Result<Unit>.Success(Unit.Value);
        }

        public async Task<Result<string>> InviteUserAsync(string email, string role)
        {
            if (_state == null)
                return Result<string>.Failure("Tenant not found");

            if (_state.Users.Any(u => u.Email == email))
                return Result<string>.Failure("User is already a member");

            var token = Guid.NewGuid().ToString("N");
            var invite = new TenantInvite
            {
                Email = email,
                Role = role,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _state.PendingInvites = _state.PendingInvites.Add(invite);
            await _storage.WriteStateAsync();

            await _emailService.SendTenantInviteEmailAsync(email, _state.Name, token);

            return Result<string>.Success(token);
        }

        public async Task<Result<Unit>> AcceptInviteAsync(string token, string userId)
        {
            if (_state == null)
                return Result<Unit>.Failure("Tenant not found");

            var invite = _state.PendingInvites.FirstOrDefault(i => i.Token == token);
            if (invite == null)
                return Result<Unit>.Failure("Invalid invite token");

            if (invite.ExpiresAt < DateTime.UtcNow)
                return Result<Unit>.Failure("Invite has expired");

            _state.Users = _state.Users.Add(new TenantUserInfo
            {
                UserId = userId,
                Role = invite.Role,
                JoinedAt = DateTime.UtcNow
            });

            _state.PendingInvites = _state.PendingInvites.Remove(invite);
            await _storage.WriteStateAsync();

            return Result<Unit>.Success(Unit.Value);
        }
    }
}