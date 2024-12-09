using Argus.Grains;
using Argus.Infrastructure.Authorization.Handlers;
using Argus.Infrastructure.Authorization.Services;
using Argus.Infrastructure.Configuration;
using Argus.Infrastructure.Data;
using Argus.Infrastructure.Extensions;
using Argus.Infrastructure.Logging;
using Argus.Infrastructure.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.IdentityModel.Tokens;
using Orleans.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.OpenSearch;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithExceptionDetails()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .WriteTo.Debug()
    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter())
.WriteTo.OpenSearch(new OpenSearchSinkOptions(new Uri(builder.Configuration["Elasticsearch:Url"]))
{
    IndexFormat = $"argus-logs-{0:yyyy.MM}",
    BatchAction = OpenOpType.Create,
    FailureCallback = e => Console.WriteLine("Failed to submit event " + e.MessageTemplate),
    EmitEventFailure = EmitEventFailureHandling.WriteToFailureSink |
                     EmitEventFailureHandling.WriteToSelfLog
})
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add API Versioning
builder.Services.AddApiVersioning(options => {
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new HeaderApiVersionReader("api-version");
});

builder.Services.AddVersionedApiExplorer(options => {
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Auth").Get<AuthConfig>();
if (string.IsNullOrEmpty(jwtSettings?.JwtSecret))
{
    throw new InvalidOperationException("JWT Secret is not configured");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(jwtSettings.JwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Configure Infrastructure
builder.Services.AddInfrastructure();

// Configure Authorization
builder.Services.AddTenantAuthorization();
builder.Services.AddScoped<IAuthorizationHandler, TenantAuthorizationHandler>();
builder.Services.AddScoped<ITenantPermissionService, TenantPermissionService>();
builder.Services.AddHttpContextAccessor();

// Add JWT service
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Configure Orleans
builder.Host.UseOrleans((context, siloBuilder) =>
{
    var orleansConfig = context.Configuration.GetSection("Orleans").Get<OrleansConfig>();

    _ = siloBuilder
        .UseLocalhostClustering(
            orleansConfig.SiloPort,
            orleansConfig.GatewayPort)
        .ConfigureServices(services =>
        {
            var manager = new ApplicationPartManager();
            manager.ApplicationParts.Add(new AssemblyPart(typeof(UserGrain).Assembly));
            services.AddSingleton(manager);
        })
        .AddMemoryGrainStorage("PubSubStore")
        .Configure<ClusterOptions>(options =>
        {
            options.ClusterId = orleansConfig.ClusterId;
            options.ServiceId = orleansConfig.ServiceId;
        });
});

// Configure Database
builder.Services.AddSingleton<IDbConnectionFactory>(sp => 
    new NpgsqlConnectionFactory(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add security headers
app.Use((context, next) =>
{
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'");
    return next();
});

// Add request logging middleware
app.UseMiddleware<RequestLogContextMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();