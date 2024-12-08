using Microsoft.EntityFrameworkCore;

namespace Argus.Infrastructure.MultiTenant
{
    public class TenantDbContext : DbContext
    {
        private readonly string _schema;

        public TenantDbContext(DbContextOptions<TenantDbContext> options, string schema)
            : base(options)
        {
            _schema = schema;
        }

        public DbSet<TenantEntity> Tenants { get; set; }
        public DbSet<UserTenantEntity> UserTenants { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (!string.IsNullOrEmpty(_schema))
            {
                modelBuilder.HasDefaultSchema(_schema);
            }

            modelBuilder.Entity<TenantEntity>(entity =>
            {
                entity.ToTable("tenants");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Subdomain).IsRequired();
                entity.HasIndex(e => e.Subdomain).IsUnique();
            });

            modelBuilder.Entity<UserTenantEntity>(entity =>
            {
                entity.ToTable("user_tenants");
                entity.HasKey(e => new { e.UserId, e.TenantId });
                entity.Property(e => e.Role).IsRequired();
                entity.Property(e => e.Status).IsRequired();
            });

            base.OnModelCreating(modelBuilder);
        }
    }

    public class TenantEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Subdomain { get; set; }
        public DateTime CreatedAt { get; set; }
        public JsonDocument Settings { get; set; }
    }

    public class UserTenantEntity
    {
        public Guid UserId { get; set; }
        public Guid TenantId { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public Guid? InvitedBy { get; set; }
        public DateTime? InvitedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
    }
}