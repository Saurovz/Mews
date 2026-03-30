using Mews.Atlas.OpenTelemetry;
using Mews.Job.Scheduler.Common.Logging.Enrichers;
using Mews.Job.Scheduler.Environments;
using NewRelic.LogEnrichers.Serilog;
using Serilog;
using Serilog.Events;

namespace Mews.Job.Scheduler.Common.Logging;

public static class Logs
{
    public static void Configure()
    {
        var baseConfiguration = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
            .MinimumLevel.Override("Temporalio.Runtime", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient.OtlpTraceExporter.LogicalHandler", LogEventLevel.Error)
            .Enrich.FromLogContext()
            .Enrich.WithNewRelicLogsInContext()
            .Enrich.With<ClassNameEnricher>()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss}] [{Level:u4}] [{ClassName}] {Message:lj}{NewLine}{Exception}"
            )
            .WriteTo.OpenTelemetryCollector();

        Log.Logger = baseConfiguration.CreateLogger();
    }
}
