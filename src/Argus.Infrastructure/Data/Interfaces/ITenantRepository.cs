using Argus.Infrastructure.Data.DTOs;

namespace Argus.Infrastructure.Data.Interfaces
{
    /// <summary>
    /// Repository interface for tenant operations
    /// </summary>
    public interface ITenantRepository
    {
        Task<TenantDto> GetByIdAsync(Guid id);
        Task<TenantDto> GetBySubdomainAsync(string subdomain);
        Task<IEnumerable<TenantDto>> GetAllAsync();
        Task<Guid> CreateAsync(TenantDto tenant);
        Task UpdateAsync(TenantDto tenant);
        Task DeleteAsync(Guid id);
    }
}