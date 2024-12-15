namespace Argus.Features.Authentication.Domain.Services
{
    public interface ITokenService
    {
        string GenerateToken(string userId, string tenantId);
        bool ValidateToken(string token);
    }

    public class TokenService : ITokenService
    {
        public string GenerateToken(string userId, string tenantId)
        {
            // Implementation will be moved here from existing code
            throw new NotImplementedException();
        }

        public bool ValidateToken(string token)
        {
            // Implementation will be moved here from existing code
            throw new NotImplementedException();
        }
    }
}