using Microsoft.AspNetCore.Authorization;
using Argus.Infrastructure.MultiTenant;

namespace Argus.Infrastructure.Auth
{
    public class TenantRequirement : IAuthorizationRequirement
    {
        public string RequiredRole { get; }

        public TenantRequirement(string requiredRole)
        {
            RequiredRole = requiredRole;
        }
    }

    public class TenantAuthorizationHandler : AuthorizationHandler<TenantRequirement>
    {
        private readonly TenantDbContext _dbContext;

        public TenantAuthorizationHandler(TenantDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            TenantRequirement requirement)
        {
            var user = context.User;
            if (user == null || !user.Identity.IsAuthenticated)
            {
                return;
            }

            var userId = Guid.Parse(user.FindFirst("sub")?.Value);
            var tenantId = GetTenantIdFromContext(context);

            if (!tenantId.HasValue)
            {
                return;
            }

            var userTenant = await _dbContext.UserTenants
                .FindAsync(userId, tenantId.Value);

            if (userTenant != null && 
                userTenant.Status == "Active" && 
                HasRequiredRole(userTenant.Role, requirement.RequiredRole))
            {
                context.Succeed(requirement);
            }
        }

        private Guid? GetTenantIdFromContext(AuthorizationHandlerContext context)
        {
            // Implementation depends on how tenant ID is passed
            // Could be from route, header, or claims
            return null;
        }

        private bool HasRequiredRole(string userRole, string requiredRole)
        {
            // Implement role hierarchy check
            return userRole == requiredRole || userRole == "Admin";
        }
    }
}