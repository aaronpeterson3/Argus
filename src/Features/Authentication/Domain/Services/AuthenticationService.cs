using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Argus.Core.Common;

namespace Argus.Features.Authentication.Domain.Services
{
    public interface IAuthenticationService
    {
        Task<Result<AuthenticationResult>> AuthenticateAsync(string email, string password);
        Task<Result<Unit>> InitiatePasswordResetAsync(string email);
        Task<Result<Unit>> ResetPasswordAsync(string token, string newPassword);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly ITokenService _tokenService;
        private readonly IUserGrain _userGrain;
        private readonly IEmailService _emailService;

        public AuthenticationService(
            ITokenService tokenService,
            IGrainFactory grainFactory,
            IEmailService emailService)
        {
            _tokenService = tokenService;
            _emailService = emailService;
        }

        public async Task<Result<AuthenticationResult>> AuthenticateAsync(string email, string password)
        {
            // Implementation moved from existing code
            var userGrain = _userGrain.GetGrain<IUserGrain>(email);
            var userState = await userGrain.GetStateAsync();

            if (userState == null)
                return Result<AuthenticationResult>.Failure("Invalid credentials");

            if (!VerifyPassword(password, userState.PasswordHash, userState.PasswordSalt))
                return Result<AuthenticationResult>.Failure("Invalid credentials");

            var token = _tokenService.GenerateToken(userState.Id, userState.TenantId);

            return Result<AuthenticationResult>.Success(new AuthenticationResult
            {
                Token = token,
                UserId = userState.Id,
                TenantId = userState.TenantId
            });
        }

        public async Task<Result<Unit>> InitiatePasswordResetAsync(string email)
        {
            var userGrain = _userGrain.GetGrain<IUserGrain>(email);
            var userState = await userGrain.GetStateAsync();

            if (userState == null)
                return Result<Unit>.Success(Unit.Value); // Don't reveal user existence

            var resetToken = await userGrain.GeneratePasswordResetTokenAsync();
            await _emailService.SendPasswordResetEmailAsync(email, resetToken);

            return Result<Unit>.Success(Unit.Value);
        }

        public async Task<Result<Unit>> ResetPasswordAsync(string token, string newPassword)
        {
            // Implementation for password reset
            throw new NotImplementedException();
        }

        private bool VerifyPassword(string password, string hash, byte[] salt)
        {
            var computedHash = Core.Infrastructure.PasswordHasher.HashPassword(password, salt);
            return computedHash == hash;
        }
    }

    public class AuthenticationResult
    {
        public string Token { get; set; }
        public string UserId { get; set; }
        public string TenantId { get; set; }
    }
}