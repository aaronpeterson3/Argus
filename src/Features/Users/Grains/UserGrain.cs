using Orleans;
using Argus.Features.Users.Domain.Models;
using Argus.Core.Common;

namespace Argus.Features.Users.Grains
{
    public interface IUserGrain : IGrainWithStringKey
    {
        Task<UserState> GetStateAsync();
        Task<Result<string>> CreateAsync(string email, string password, UserProfile profile);
        Task<Result<Unit>> UpdateProfileAsync(UserProfile profile);
        Task<Result<Unit>> ChangePasswordAsync(string currentPassword, string newPassword);
        Task<string> GeneratePasswordResetTokenAsync();
        Task<Result<Unit>> AddTenantAsync(string tenantId);
    }

    public class UserGrain : Grain, IUserGrain
    {
        private UserState _state;
        private readonly IPersistentState<UserState> _storage;

        public UserGrain([PersistentState("user")] IPersistentState<UserState> storage)
        {
            _storage = storage;
        }

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await base.OnActivateAsync(cancellationToken);
            _state = _storage.State;
        }

        public Task<UserState> GetStateAsync()
        {
            return Task.FromResult(_state);
        }

        public async Task<Result<string>> CreateAsync(string email, string password, UserProfile profile)
        {
            if (_state != null)
                return Result<string>.Failure("User already exists");

            var salt = new byte[16];
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            var passwordHash = Core.Infrastructure.PasswordHasher.HashPassword(password, salt);

            _state = new UserState
            {
                Id = this.GetPrimaryKeyString(),
                Email = email,
                PasswordHash = passwordHash,
                PasswordSalt = salt,
                Profile = profile,
                CreatedAt = DateTime.UtcNow
            };

            await _storage.WriteStateAsync();
            return Result<string>.Success(_state.Id);
        }

        public async Task<Result<Unit>> UpdateProfileAsync(UserProfile profile)
        {
            if (_state == null)
                return Result<Unit>.Failure("User not found");

            _state.Profile = profile;
            _state.Profile.UpdatedAt = DateTime.UtcNow;

            await _storage.WriteStateAsync();
            return Result<Unit>.Success(Unit.Value);
        }

        public async Task<Result<Unit>> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            if (_state == null)
                return Result<Unit>.Failure("User not found");

            var currentHash = Core.Infrastructure.PasswordHasher.HashPassword(currentPassword, _state.PasswordSalt);
            if (currentHash != _state.PasswordHash)
                return Result<Unit>.Failure("Invalid current password");

            var newSalt = new byte[16];
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                rng.GetBytes(newSalt);
            }

            _state.PasswordHash = Core.Infrastructure.PasswordHasher.HashPassword(newPassword, newSalt);
            _state.PasswordSalt = newSalt;

            await _storage.WriteStateAsync();
            return Result<Unit>.Success(Unit.Value);
        }

        public async Task<string> GeneratePasswordResetTokenAsync()
        {
            if (_state == null)
                return null;

            _state.PasswordResetToken = Guid.NewGuid().ToString("N");
            _state.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(24);

            await _storage.WriteStateAsync();
            return _state.PasswordResetToken;
        }

        public async Task<Result<Unit>> AddTenantAsync(string tenantId)
        {
            if (_state == null)
                return Result<Unit>.Failure("User not found");

            if (!_state.TenantIds.Contains(tenantId))
            {
                _state.TenantIds.Add(tenantId);
                if (_state.CurrentTenantId == null)
                    _state.CurrentTenantId = tenantId;

                await _storage.WriteStateAsync();
            }

            return Result<Unit>.Success(Unit.Value);
        }
    }
}