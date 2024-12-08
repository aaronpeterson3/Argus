using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Orleans;
using Argus.Abstractions;
using Argus.Api;

namespace Argus.Grains
{
    /// <summary>
    /// Implementation of the user management grain
    /// </summary>
    public class UserGrain : Grain, IUserGrain
    {
        private readonly PasswordHashConfig _passwordConfig;
        private readonly ILogger<UserGrain> _logger;
        private UserState state;
        private string passwordHash;

        /// <summary>
        /// Initializes a new instance of the UserGrain class
        /// </summary>
        public UserGrain(IOptions<OrleansConfig> options, ILogger<UserGrain> logger)
        {
            _passwordConfig = options.Value.PasswordHashConfig;
            _logger = logger;
        }

        /// <inheritdoc/>
        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Activating UserGrain for {UserId}", this.GetPrimaryKeyString());
            await base.OnActivateAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> CreateAsync(string email, string password, UserProfile profile)
        {
            try
            {
                if (state != null)
                {
                    _logger.LogWarning("Attempted to create user that already exists: {Email}", email);
                    return false;
                }

                passwordHash = HashPassword(password);
                state = new UserState
                {
                    Email = email,
                    Profile = profile,
                    IsActive = true
                };

                _logger.LogInformation("Successfully created user: {Email}", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create user: {Email}", email);
                throw;
            }
        }

        /// <inheritdoc/>
        public Task<bool> ValidateCredentialsAsync(string password)
        {
            try
            {
                if (state == null)
                {
                    _logger.LogWarning("Attempted to validate non-existent user: {UserId}", this.GetPrimaryKeyString());
                    return Task.FromResult(false);
                }

                var isValid = VerifyPassword(password, passwordHash);
                _logger.LogInformation(
                    isValid ? "Successful login attempt for {Email}" : "Failed login attempt for {Email}", 
                    state.Email);

                return Task.FromResult(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during credential validation for {UserId}", this.GetPrimaryKeyString());
                throw;
            }
        }

        /// <inheritdoc/>
        public Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            try
            {
                if (!VerifyPassword(currentPassword, passwordHash))
                {
                    _logger.LogWarning("Failed password change attempt for {Email}", state?.Email);
                    return Task.FromResult(false);
                }

                passwordHash = HashPassword(newPassword);
                _logger.LogInformation("Successfully changed password for {Email}", state.Email);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password change for {Email}", state?.Email);
                throw;
            }
        }

        /// <inheritdoc/>
        public Task<UserState> GetStateAsync()
        {
            _logger.LogDebug("Retrieved state for {Email}", state?.Email);
            return Task.FromResult(state);
        }

        private string HashPassword(string password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: _passwordConfig.IterationCount,
                numBytesRequested: _passwordConfig.NumBytesRequested));

            return $"{Convert.ToBase64String(salt)}.{hashed}";
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            var parts = hashedPassword.Split('.');
            if (parts.Length != 2)
                return false;

            var salt = Convert.FromBase64String(parts[0]);
            var hash = parts[1];

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: _passwordConfig.IterationCount,
                numBytesRequested: _passwordConfig.NumBytesRequested));

            return hash == hashed;
        }
    }
}