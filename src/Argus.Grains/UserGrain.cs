using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;
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
        private UserState state;
        private string passwordHash;

        /// <summary>
        /// Initializes a new instance of the UserGrain class
        /// </summary>
        /// <param name="options">Orleans configuration options</param>
        public UserGrain(IOptions<OrleansConfig> options)
        {
            _passwordConfig = options.Value.PasswordHashConfig;
        }

        /// <inheritdoc/>
        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await base.OnActivateAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> CreateAsync(string email, string password, UserProfile profile)
        {
            if (state != null)
                return false;

            passwordHash = HashPassword(password);
            state = new UserState
            {
                Email = email,
                Profile = profile,
                IsActive = true
            };

            return true;
        }

        /// <inheritdoc/>
        public Task<bool> ValidateCredentialsAsync(string password)
        {
            if (state == null)
                return Task.FromResult(false);

            return Task.FromResult(VerifyPassword(password, passwordHash));
        }

        /// <inheritdoc/>
        public Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            if (!VerifyPassword(currentPassword, passwordHash))
                return Task.FromResult(false);

            passwordHash = HashPassword(newPassword);
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<UserState> GetStateAsync()
        {
            return Task.FromResult(state);
        }

        /// <summary>
        /// Hashes a password using PBKDF2 with a random salt
        /// </summary>
        /// <param name="password">Password to hash</param>
        /// <returns>Combined salt and hash string</returns>
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

        /// <summary>
        /// Verifies a password against a stored hash
        /// </summary>
        /// <param name="password">Password to verify</param>
        /// <param name="hashedPassword">Stored password hash with salt</param>
        /// <returns>True if password matches, false otherwise</returns>
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