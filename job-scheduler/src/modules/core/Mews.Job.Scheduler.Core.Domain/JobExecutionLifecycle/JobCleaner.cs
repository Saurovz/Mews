using System.Diagnostics;
using Mews.Job.Scheduler.Domain.DateTimeProviders;
using Mews.Job.Scheduler.Observability;
using Microsoft.Extensions.Logging;

namespace Mews.Job.Scheduler.Domain.JobExecutionLifecycle;

public sealed class JobCleaner
{
    private readonly IJobPersistence _persistence;
    private readonly ILogger<JobCleaner> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly JobCleanerMetrics _metrics;

    public JobCleaner(
        IJobPersistence persistence,
        ILogger<JobCleaner> logger,
        IDateTimeProvider dateTimeProvider,
        JobCleanerMetrics metrics)
    {
        _persistence = persistence;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
        _metrics = metrics;
    }

    public async Task CleanAsync(int retentionDays, CancellationToken cancellationToken)
    {
        var startTimestamp = Stopwatch.GetTimestamp();

        var deletedCount = await _persistence.Jobs.RemoveJobsFollowingRetentionPolicy(
            _dateTimeProvider.NowUtc.AddDays(-retentionDays), cancellationToken);

        _logger.LogInformation("Deleted {Count} jobs", deletedCount);
        _metrics.RecordExecutionMetrics(deletedCount, Stopwatch.GetElapsedTime(startTimestamp));
    }
}
