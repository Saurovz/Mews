using System.Diagnostics.Metrics;

namespace Mews.Job.Scheduler.Observability;

public class JobExecutionCleanerMetrics
{
    public const string MeterName = "Mews.Job.Scheduler.JobExecutionCleaner";

    private readonly Counter<int> _jobExecutionCleanerTotalDeleted;
    private readonly Histogram<double> _executionTime;

    public JobExecutionCleanerMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);

        _jobExecutionCleanerTotalDeleted = meter.CreateCounter<int>("mews.job.scheduler.jobExecutionCleaner.deleted", "job",
            "Total number of deleted job executions");
        _executionTime = meter.CreateHistogram<double>("mews.job.scheduler.jobExecutionCleaner.executionTime", "milliseconds",
            "Execution time of the job execution cleaner");
    }

    public void RecordExecutionMetrics(int deletedEntitiesCount, TimeSpan executionDuration)
    {
        _jobExecutionCleanerTotalDeleted.Add(deletedEntitiesCount);
        _executionTime.Record(executionDuration.TotalMilliseconds);
    }
}
