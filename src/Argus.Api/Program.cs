using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Enrichers.Thread;
using Serilog.Enrichers.Process;
using Serilog.Sinks.Elasticsearch;
using Argus.Infrastructure.Logging;
using Argus.Infrastructure.Authorization.Cedar;
using Argus.Infrastructure.Data.Interfaces;
using Argus.Infrastructure.Data.Repositories;
using Argus.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Orleans.Runtime;
using Orleans.Hosting;
using Orleans.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(builder.Configuration["Elasticsearch:Url"]))
    {
        AutoRegisterTemplate = true,
        IndexFormat = $"argus-logs-{0:yyyy.MM}",
        InlineFields = true,
        EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                         EmitEventFailureHandling.WriteToFailureSink |
                         EmitEventFailureHandling.RaiseCallback
    })
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Auth").Get<AuthConfig>();
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

// Configure Orleans
builder.Host.UseOrleans((context, siloBuilder) =>
{
    var orleansConfig = context.Configuration.GetSection("Orleans").Get<OrleansConfig>();
    
    siloBuilder
        .UseLocalhostClustering(
            orleansConfig.SiloPort,
            orleansConfig.GatewayPort)
        .ConfigureServices(services =>
        {
            services.AddApplicationPart(typeof(UserGrain).Assembly).WithReferences();
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

// Configure Authorization
builder.Services.Configure<CedarAuthorizationOptions>(builder.Configuration.GetSection("Cedar"));
builder.Services.AddScoped<IAuthorizationService, CedarAuthorization>();

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