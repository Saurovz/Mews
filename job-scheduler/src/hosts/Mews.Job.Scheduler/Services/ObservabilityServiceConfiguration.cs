using Mews.Atlas.OpenTelemetry;
using Mews.Atlas.Temporal;
using Mews.Job.Scheduler.Common.OpenTelemetry;
using Mews.Job.Scheduler.Core.EntityFrameworkCore.Observability;
using Mews.Job.Scheduler.Observability;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;


namespace Mews.Job.Scheduler.Services;

public static class ObservabilityServiceConfiguration
{
    public static IServiceCollection AddOpenTelemetryServices(this IServiceCollection services)
    {
        var meters = OpenTelemetryMeters.GetMeters();
        var resourceBuilder = ResourceBuilder.CreateDefault().AddAzureContainerAppAttributes();

        services
            .AddOpenTelemetry()
            .AddTracing(
                configureProvider: builder => builder
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        options.SetDbStatementForText = true;
                    })
                    .AddTemporalInstrumentation()
                    .AddSource(
                        JobSchedulerDiagnostics.ActivitySourceName,
                        JobProcessingDiagnostics.ActivitySourceName,
                        EfCoreDiagnostics.ActivitySourceName
                    )
            )
            .AddMetrics(
                meters: meters,
                configureProvider: builder => builder
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddTemporalInstrumentation()
            );

        return services;
    }
}
