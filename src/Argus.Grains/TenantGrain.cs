using Orleans;
using Argus.Infrastructure.MultiTenant;
using Microsoft.Extensions.Logging;

namespace Argus.Grains
{
    public class TenantGrain : Grain, ITenantGrain
    {
        private readonly ILogger<TenantGrain> _logger;
        private readonly TenantDbContext _dbContext;
        private TenantInfo _tenantInfo;

        public TenantGrain(ILogger<TenantGrain> logger, TenantDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public override async Task OnActivateAsync(CancellationToken token)
        {
            var tenantId = this.GetPrimaryKeyGuid();
            var tenant = await _dbContext.Tenants.FindAsync(tenantId);
            
            if (tenant != null)
            {
                _tenantInfo = new TenantInfo
                {
                    Id = tenant.Id,
                    Name = tenant.Name,
                    Subdomain = tenant.Subdomain,
                    Settings = tenant.Settings.Deserialize<Dictionary<string, object>>()
                };
            }

            await base.OnActivateAsync(token);
        }

        public async Task<bool> InviteUser(string email, string role)
        {
            try
            {
                var userTenant = new UserTenantEntity
                {
                    TenantId = _tenantInfo.Id,
                    Role = role,
                    Status = "Pending",
                    InvitedAt = DateTime.UtcNow,
                    InvitedBy = GetInvitingUserId()
                };

                _dbContext.UserTenants.Add(userTenant);
                await _dbContext.SaveChangesAsync();

                // Send invitation email
                await SendInvitationEmail(email);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to invite user {Email} to tenant {TenantId}", email, _tenantInfo.Id);
                return false;
            }
        }

        private async Task SendInvitationEmail(string email)
        {
            // Implement email sending logic
        }
    }
}