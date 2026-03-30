using Mews.Atlas.OpenTelemetry;
using NewRelic.LogEnrichers.Serilog;
using Serilog;

namespace TaxManager.Extensions;

public static class LoggerConfigurationExtensions
{
    public static LoggerConfiguration ConfigureLogging(this LoggerConfiguration baseConfiguration)
    {
        baseConfiguration
            .Enrich.FromLogContext()
            .Enrich.WithNewRelicLogsInContext()
            .WriteTo.OpenTelemetryCollector()
            .WriteTo.Console(); 
        
        return baseConfiguration;
    }
}
