using System.Diagnostics.Metrics;

namespace Mews.Job.Scheduler.Observability;

public class JobCleanerMetrics
{
    public const string MeterName = "Mews.Job.Scheduler.JobCleaner";

    private readonly Counter<int> _totalDeletedEntities;
    private readonly Histogram<double> _executionTime;

    public JobCleanerMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);

        _totalDeletedEntities = meter.CreateCounter<int>("mews.job.scheduler.jobCleaner.deleted", "job",
            "Total number of deleted jobs");
        _executionTime = meter.CreateHistogram<double>("mews.job.scheduler.jobCleaner.executionTime", "milliseconds",
            "Execution time of the job cleaner");
    }

    public void RecordExecutionMetrics(int deletedEntitiesCount, TimeSpan executionDuration)
    {
        _totalDeletedEntities.Add(deletedEntitiesCount);
        _executionTime.Record(executionDuration.TotalMilliseconds);
    }
}
