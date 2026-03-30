using System.Diagnostics.Metrics;

namespace Mews.Job.Scheduler.Observability;

public class ExecutorCleanerMetrics
{
    public const string MeterName = "Mews.Job.Scheduler.ExecutorCleaner";
    
    private readonly Counter<int> _totalDeletedEntities;
    private readonly Histogram<double> _executionTime;

    public ExecutorCleanerMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);

        _totalDeletedEntities = meter.CreateCounter<int>("mews.job.scheduler.executorCleaner.deleted", "executor", "Total number of deleted executors");
        _executionTime = meter.CreateHistogram<double>("mews.job.scheduler.executorCleaner.executionTime", "milliseconds", "Execution time of the job cleaner");
    }

    public void RecordExecutionMetrics(int deletedEntityCount, TimeSpan executionDuration)
    {
        _totalDeletedEntities.Add(deletedEntityCount);
        _executionTime.Record(executionDuration.TotalMilliseconds);
    }
}
