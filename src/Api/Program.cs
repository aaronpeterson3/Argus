using Argus.Core.Infrastructure;
using Argus.Core.Infrastructure.Middleware;
using Argus.Core.Infrastructure.Health;
using Argus.Core.Infrastructure.CQRS;
using Argus.Features.Authentication.Infrastructure;
using Argus.Features.Users.Infrastructure;
using Argus.Features.Tenants.Infrastructure;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Core services
builder.Services.AddCoreServices();
builder.Services.AddHttpContextAccessor();

// Add Health Checks
builder.Services.AddHealthChecks(builder.Configuration);

// Add MediatR with CQRS behaviors
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    // Register features assemblies
    cfg.RegisterServicesFromAssemblyContaining<Argus.Features.Authentication.Api.AuthenticationController>();
    cfg.RegisterServicesFromAssemblyContaining<Argus.Features.Users.Api.UsersController>();
    cfg.RegisterServicesFromAssemblyContaining<Argus.Features.Tenants.Api.TenantsController>();

    // Add pipeline behaviors
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
});

// Add Validation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddValidatorsFromAssemblyContaining<Argus.Features.Authentication.Api.AuthenticationController>();
builder.Services.AddValidatorsFromAssemblyContaining<Argus.Features.Users.Api.UsersController>();
builder.Services.AddValidatorsFromAssemblyContaining<Argus.Features.Tenants.Api.TenantsController>();

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
        siloBuilder.UseAzureStorageClustering(options =>
            options.ConnectionString = builder.Configuration.GetConnectionString("Storage"));
        siloBuilder.AddAzureTableGrainStorage("tenant-store", options =>
            options.ConnectionString = builder.Configuration.GetConnectionString("Storage"));
        siloBuilder.AddAzureTableGrainStorage("user-store", options =>
            options.ConnectionString = builder.Configuration.GetConnectionString("Storage"));
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
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});

// Add Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("Admin"));
    
    options.AddPolicy("RequireTenantAccess", policy =>
        policy.RequireClaim("tenant_id"));
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add health checks endpoint
app.UseHealthChecks();

// Add middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<ActivityLoggingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseMiddleware<TenantMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();