using Argus.Infrastructure.Data.DTOs;
using Argus.Infrastructure.Data.Interfaces;
using Argus.Infrastructure.Encryption;
using Dapper;
using static Argus.Infrastructure.Data.SqlQueries;

namespace Argus.Infrastructure.Data.Repositories;

public sealed class TenantRepository : ITenantRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IDataEncryption _encryption;

    public TenantRepository(IDbConnectionFactory connectionFactory, IDataEncryption encryption)
    {
        _connectionFactory = connectionFactory;
        _encryption = encryption;
    }

    public async Task<TenantDto> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<TenantDto>(
            Tenants.SelectById,
            new { Id = id });
    }

    public async Task<TenantDto> GetBySubdomainAsync(string subdomain)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<TenantDto>(
            Tenants.SelectBySubdomain,
            new { Subdomain = subdomain });
    }

    public async Task<IEnumerable<TenantDto>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<TenantDto>(Tenants.SelectAll);
    }

    public async Task<Guid> CreateAsync(TenantDto tenant)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            var newTenant = new TenantDto(
                Guid.NewGuid(),
                tenant.Name,
                tenant.Subdomain,
                tenant.LogoUrl);

            await connection.ExecuteAsync(
                Tenants.Insert,
                newTenant,
                transaction);

            transaction.Commit();
            return newTenant.Id;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task UpdateAsync(TenantDto tenant)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(Tenants.Update, tenant);
    }

    public async Task DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(Tenants.Delete, new { Id = id });
    }

    public async Task<bool> HasPermissionAsync(Guid userId, Guid tenantId, string permission)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QuerySingleOrDefaultAsync<int>(
            Tenants.CheckPermission,
            new { UserId = userId, TenantId = tenantId, Permission = permission });

        return result > 0;
    }
}