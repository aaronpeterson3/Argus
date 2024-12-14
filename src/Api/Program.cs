using Argus.Core.Infrastructure;
using Argus.Core.Infrastructure.Middleware;
using Argus.Features.Authentication.Infrastructure;
using Argus.Features.Users.Infrastructure;
using Argus.Features.Tenants.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Core services
builder.Services.AddCoreServices();
builder.Services.AddHttpContextAccessor();

// Add Feature services
builder.Services.AddAuthenticationFeature();
builder.Services.AddUsersFeature();
builder.Services.AddTenantsFeature();

// Add Orleans
builder.Host.UseOrleans(siloBuilder =>
{
    if (builder.Environment.IsDevelopment())
    {
        siloBuilder.UseLocalhostClustering();
        siloBuilder.AddMemoryGrainStorage("tenant-store");
        siloBuilder.AddMemoryGrainStorage("user-store");
    }
    else
    {
        // Production Orleans configuration
        // Add Azure/Redis/etc. clustering and storage
    }
});

// Add Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["Auth:Authority"];
    options.Audience = builder.Configuration["Auth:Audience"];
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<ActivityLoggingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseMiddleware<TenantMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();