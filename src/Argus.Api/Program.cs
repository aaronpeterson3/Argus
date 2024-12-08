using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Enrichers.Thread;
using Serilog.Enrichers.Process;
using Serilog.Sinks.Elasticsearch;
using Argus.Infrastructure.Logging;

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

// Rest of Program.cs remains the same