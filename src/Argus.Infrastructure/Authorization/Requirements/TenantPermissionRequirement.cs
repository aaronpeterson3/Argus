using Microsoft.AspNetCore.Authorization;

namespace Argus.Infrastructure.Authorization.Requirements;

public class TenantPermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }
    public TenantPermissionRequirement(string permission) => Permission = permission;
}

public static class TenantPermissions
{
    public const string ViewData = "Data.View";
    public const string EditData = "Data.Edit";
    public const string DeleteData = "Data.Delete";
    public const string ManageUsers = "Users.Manage";
    public const string ViewReports = "Reports.View";
    public const string ManageSettings = "Settings.Manage";
}