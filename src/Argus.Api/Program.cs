using Argus.Grains;
using Argus.Infrastructure.Authorization.Handlers;
using Argus.Infrastructure.Authorization.Services;
using Argus.Infrastructure.Configuration;
using Argus.Infrastructure.Data;
using Argus.Infrastructure.Encryption;
using Argus.Infrastructure.Extensions;
using Argus.Infrastructure.HealthChecks;
using Argus.Infrastructure.Logging;
using Argus.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using Orleans.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.OpenSearch;
using System.IO.Compression;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure all Configuration objects
builder.Services.Configure<AuthConfig>(
    builder.Configuration.GetSection("Auth"));
builder.Services.Configure<OrleansConfig>(
    builder.Configuration.GetSection("Orleans"));
builder.Services.Configure<OpenSearchOptions>(
    builder.Configuration.GetSection("Elasticsearch"));

// 2. Configure Serilog
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

// 3. Core Services
builder.Services.AddHttpContextAccessor();

// Database
builder.Services.AddSingleton<IDbConnectionFactory>(sp => 
    new NpgsqlConnectionFactory(builder.Configuration.GetConnectionString("DefaultConnection")));

// Encryption
builder.Services.AddSingleton<IDataEncryption, DataEncryption>();

// 4. Infrastructure Services
builder.Services.AddInfrastructure();

// 5. Business Services
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<ITenantLogoStorage, TenantLogoStorage>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// 6. Authentication & Authorization
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

builder.Services.AddTenantAuthorization();
builder.Services.AddScoped<IAuthorizationHandler, TenantAuthorizationHandler>();
builder.Services.AddScoped<ITenantPermissionService, TenantPermissionService>();

// 7. API Features
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// API Versioning
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

// 8. Additional Features
// CORS
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.EnableForHttps = true;
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

// Rate Limiting
builder.Services.AddRateLimiter(options => {
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection"))
    .AddCheck<OrleansHealthCheck>("Orleans")
    .AddCheck<OpenSearchHealthCheck>("OpenSearch");

// 9. Orleans Configuration
builder.Host.UseOrleans((context, siloBuilder) =>
{
    var orleansConfig = context.Configuration.GetSection("Orleans").Get<OrleansConfig>();

    _ = siloBuilder
        .UseLocalhostClustering(
            orleansConfig?.SiloPort ?? 11111,
            orleansConfig?.GatewayPort ?? 30000)
        .ConfigureServices(services =>
        {
            var manager = new ApplicationPartManager();
            manager.ApplicationParts.Add(new AssemblyPart(typeof(UserGrain).Assembly));
            services.AddSingleton(manager);
        })
        .AddMemoryGrainStorage("PubSubStore")
        .Configure<ClusterOptions>(options =>
        {
            options.ClusterId = orleansConfig?.ClusterId ?? "dev";
            options.ServiceId = orleansConfig?.ServiceId ?? "ArgusService";
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 1. Exception Handling
app.UseExceptionHandler("/error");

// 2. Security Headers
app.Use((context, next) =>
{
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'");
    context.Response.Headers.Add("X-Permitted-Cross-Domain-Policies", "none");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    return next();
});

// 3. Basic Security
app.UseHttpsRedirection();
app.UseCors();

// 4. Response Compression
app.UseResponseCompression();

// 5. Rate Limiting
app.UseRateLimiter();

// 6. Routing
app.UseRouting();

// 7. Request Pipeline
app.UseMiddleware<RequestLogContextMiddleware>();

// 8. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 9. Endpoints
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();