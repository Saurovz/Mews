using System.Diagnostics;
using Mews.Job.Scheduler.Domain.DateTimeProviders;
using Mews.Job.Scheduler.Observability;
using Microsoft.Extensions.Logging;

namespace Mews.Job.Scheduler.Domain.JobExecutionLifecycle;

public sealed class JobExecutionCleaner
{
    private readonly IJobPersistence _persistence;
    private readonly ILogger<JobExecutionCleaner> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly JobExecutionCleanerMetrics _metrics;

    public JobExecutionCleaner(
        IJobPersistence persistence,
        ILogger<JobExecutionCleaner> logger,
        IDateTimeProvider dateTimeProvider,
        JobExecutionCleanerMetrics metrics)
    {
        _persistence = persistence;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
        _metrics = metrics;
    }

    public async Task CleanAsync(int retentionDays, CancellationToken cancellationToken)
    {
        var startTimestamp = Stopwatch.GetTimestamp();

        var deletedCount = await _persistence.JobExecutions.RemoveJobExecutionsOlderThanAsync(
            _dateTimeProvider.NowUtc.AddDays(-retentionDays), cancellationToken);

        _logger.LogInformation("Deleted {Count} job executions", deletedCount);
        _metrics.RecordExecutionMetrics(deletedCount, Stopwatch.GetElapsedTime(startTimestamp));
    }
}
