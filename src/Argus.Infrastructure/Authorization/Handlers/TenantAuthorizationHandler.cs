using Microsoft.AspNetCore.Authorization;

namespace Argus.Infrastructure.Authorization.Handlers
{
    public class TenantAuthorizationHandler : AuthorizationHandler<TenantRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            TenantRequirement requirement)
        {
            // Handler implementation
            return Task.CompletedTask;
        }
    }

    public class TenantRequirement : IAuthorizationRequirement { }
}