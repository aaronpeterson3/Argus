using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using Argus.Infrastructure.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithExceptionDetails()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironment(builder.Environment.EnvironmentName)
    .WriteTo.Debug()
    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter())
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
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

// Add logging services
builder.Services.AddScoped<AuditLogger>();
builder.Services.AddScoped<MetricsLogger>();

var app = builder.Build();

// Add request logging middleware
app.UseMiddleware<RequestLogContextMiddleware>();

// Global exception handler
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Unhandled exception occurred");
        throw;
    }
});

// Rest of your Program.cs configuration
