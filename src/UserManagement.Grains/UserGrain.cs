using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Orleans;
using UserManagement.Abstractions;

namespace UserManagement.Grains
{
    public class UserGrain : Grain, IUserGrain
    {
        private UserState state;
        private string passwordHash;

        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await base.OnActivateAsync(cancellationToken);
        }

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

        public Task<bool> ValidateCredentialsAsync(string password)
        {
            if (state == null)
                return Task.FromResult(false);

            return Task.FromResult(VerifyPassword(password, passwordHash));
        }

        public Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            if (!VerifyPassword(currentPassword, passwordHash))
                return Task.FromResult(false);

            passwordHash = HashPassword(newPassword);
            return Task.FromResult(true);
        }

        public Task<UserState> GetStateAsync()
        {
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
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

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
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            return hash == hashed;
        }
    }
}