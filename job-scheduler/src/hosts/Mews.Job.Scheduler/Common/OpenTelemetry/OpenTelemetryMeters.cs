using Mews.Atlas.Messaging.ServiceBus.Telemetry;
using Mews.Job.Scheduler.Observability;

namespace Mews.Job.Scheduler.Common.OpenTelemetry;

public static class OpenTelemetryMeters
{
    public static string[] GetMeters()
    {
        return
        [
            MessagingMetrics.MeterName,
            JobSchedulerMetrics.MeterName,
            ExecutorCleanerMetrics.MeterName,
            JobCleanerMetrics.MeterName,
            JobExecutionCleanerMetrics.MeterName,
            JobTimeoutHandlerMetrics.MeterName
        ];
    }
}
