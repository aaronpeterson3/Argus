namespace Argus.Infrastructure.Data;

public static class SqlQueries
{
    public static class Tenants
    {
        public const string SelectById = """
            SELECT id, name, subdomain, logo_url, created_at, settings
            FROM tenants
            WHERE id = @Id
            """;

        public const string SelectBySubdomain = """
            SELECT id, name, subdomain, logo_url, created_at, settings
            FROM tenants
            WHERE subdomain = @Subdomain
            """;

        public const string SelectAll = """
            SELECT id, name, subdomain, logo_url, created_at, settings
            FROM tenants
            """;

        public const string Insert = """
            INSERT INTO tenants (id, name, subdomain, logo_url, created_at, settings)
            VALUES (@Id, @Name, @Subdomain, @LogoUrl, @CreatedAt, @Settings::jsonb)
            RETURNING id
            """;
            
        public const string Update = """
            UPDATE tenants
            SET name = @Name,
                subdomain = @Subdomain,
                logo_url = @LogoUrl,
                settings = @Settings::jsonb
            WHERE id = @Id
            """;
            
        public const string Delete = """
            DELETE FROM tenants
            WHERE id = @Id
            """;
    }
}