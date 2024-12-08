using Argus.Infrastructure.Data.DTOs;

namespace Argus.Infrastructure.Data.Interfaces
{
    /// <summary>
    /// Repository interface for user operations
    /// </summary>
    public interface IUserRepository
    {
        Task<UserDto> GetByIdAsync(Guid id);
        Task<UserDto> GetByEmailAsync(string email);
        Task<IEnumerable<UserDto>> GetByTenantAsync(Guid tenantId);
        Task<Guid> CreateAsync(UserDto user);
        Task UpdateAsync(UserDto user);
        Task DeleteAsync(Guid id);
        Task<bool> ValidateCredentialsAsync(string email, string password);
    }
}