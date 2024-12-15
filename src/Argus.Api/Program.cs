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
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Orleans.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.OpenSearch;
using System.IO.Compression;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configure all Configuration objects
builder.Services.Configure<AuthConfig>(
    builder.Configuration.GetSection("Auth"));
builder.Services.Configure<OrleansConfig>(
    builder.Configuration.GetSection("Orleans"));
builder.Services.Configure<OpenSearchOptions>(
    builder.Configuration.GetSection("Elasticsearch"));

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

// Core Services
builder.Services.AddHttpContextAccessor();

// Database
builder.Services.AddSingleton<IDbConnectionFactory>(sp => 
    new NpgsqlConnectionFactory(builder.Configuration.GetConnectionString("DefaultConnection")));

// Encryption
builder.Services.AddSingleton<IDataEncryption, DataEncryption>();

// Infrastructure Services
builder.Services.AddInfrastructure();

// Business Services
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<ITenantLogoStorage, TenantLogoStorage>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Authentication & Authorization
var jwtSettings = builder.Configuration.GetSection("Auth").Get<AuthConfig>();
if (string.IsNullOrEmpty(jwtSettings?.JwtSecret))
{
    throw new InvalidOperationException("JWT Secret is not configured");
}

builder.Services.AddAuthentication()
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

// API Features
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Argus API",
        Version = "v1",
        Description = "API for Argus multi-tenant application"
    });

    // Add JWT Authentication support in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Enable XML Comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// API Versioning
builder.Services.AddApiVersioning(options => {
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new HeaderApiVersionReader("api-version");
}).AddApiExplorer(options => {
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

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

// Orleans Configuration
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
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Argus API v1");
        c.RoutePrefix = string.Empty; // Serve the Swagger UI at the root URL
    });
}

// Exception Handling
app.UseExceptionHandler("/error");

// Security Headers
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

// Basic Security
app.UseHttpsRedirection();
app.UseCors();

// Response Compression
app.UseResponseCompression();

// Rate Limiting
app.UseRateLimiter();

// Routing
app.UseRouting();

// Request Pipeline
app.UseMiddleware<RequestLogContextMiddleware>();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();