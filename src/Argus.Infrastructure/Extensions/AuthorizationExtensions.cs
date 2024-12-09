using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Argus.Infrastructure.Authorization.Requirements;
using Argus.Infrastructure.Authorization.Handlers;

namespace Argus.Infrastructure.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddTenantAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("ViewData", policy =>
                policy.Requirements.Add(new TenantPermissionRequirement(TenantPermissions.ViewData)));

            options.AddPolicy("EditData", policy =>
                policy.Requirements.Add(new TenantPermissionRequirement(TenantPermissions.EditData)));

            options.AddPolicy("DeleteData", policy =>
                policy.Requirements.Add(new TenantPermissionRequirement(TenantPermissions.DeleteData)));

            options.AddPolicy("ManageUsers", policy =>
                policy.Requirements.Add(new TenantPermissionRequirement(TenantPermissions.ManageUsers)));

            options.AddPolicy("ViewReports", policy =>
                policy.Requirements.Add(new TenantPermissionRequirement(TenantPermissions.ViewReports)));

            options.AddPolicy("ManageSettings", policy =>
                policy.Requirements.Add(new TenantPermissionRequirement(TenantPermissions.ManageSettings)));
        });

        return services;
    }
}