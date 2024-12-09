// Update authorization section
builder.Services.AddTenantAuthorization();
builder.Services.AddScoped<IAuthorizationHandler, TenantAuthorizationHandler>();
builder.Services.AddScoped<ITenantPermissionService, TenantPermissionService>();
builder.Services.AddHttpContextAccessor();

// Example controller usage:
[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    [HttpGet("{tenantId}/data")]
    [Authorize(Policy = "ViewData")]
    public async Task<IActionResult> GetData(Guid tenantId)
    {
        // Authorization middleware will check if user has ViewData permission for this tenant
        return Ok(new { message = "Data accessed" });
    }

    [HttpPost("{tenantId}/data")]
    [Authorize(Policy = "EditData")]
    public async Task<IActionResult> CreateData(Guid tenantId, [FromBody] object data)
    {
        // Authorization middleware will check if user has EditData permission for this tenant
        return Ok(new { message = "Data created" });
    }
}