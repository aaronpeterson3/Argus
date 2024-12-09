using Dapper;
using Argus.Infrastructure.Data.DTOs;
using Argus.Infrastructure.Data.Interfaces;
using static Argus.Infrastructure.Data.SqlQueries;

namespace Argus.Infrastructure.Data.Repositories;

public sealed class TenantRepository(IDbConnectionFactory connectionFactory, IDataEncryption encryption) : ITenantRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
    private readonly IDataEncryption _encryption = encryption;
    private static readonly string[] BasicColumns = ["id", "name", "subdomain", "logo_url", "created_at", "settings"];

    public async Task<TenantDto> GetByIdAsync(TenantId id)
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

    public async Task<TenantId> CreateAsync(TenantDto tenant)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            var id = await connection.QuerySingleAsync<TenantId>(
                Tenants.Insert,
                tenant with { Id = TenantId.NewGuid() },
                transaction);

            await transaction.CommitAsync();
            return id;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}