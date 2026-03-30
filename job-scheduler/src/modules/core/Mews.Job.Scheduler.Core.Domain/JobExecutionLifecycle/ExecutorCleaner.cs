using System.Diagnostics;
using Mews.Job.Scheduler.Domain.DateTimeProviders;
using Mews.Job.Scheduler.Domain.Executors;
using Mews.Job.Scheduler.Observability;
using Microsoft.Extensions.Logging;

namespace Mews.Job.Scheduler.Domain.JobExecutionLifecycle;

public class ExecutorCleaner
{
    private readonly IExecutorRepository _executorRepository;
    private readonly ILogger<ExecutorCleaner> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ExecutorCleanerMetrics _metrics;

    public ExecutorCleaner(
        IExecutorRepository executorRepository,
        ILogger<ExecutorCleaner> logger,
        IDateTimeProvider dateTimeProvider,
        ExecutorCleanerMetrics metrics)
    {
        _executorRepository = executorRepository;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
        _metrics = metrics;
    }
    
    public async Task CleanAsync(int retentionDays, CancellationToken cancellationToken)
    {
        var startTimestamp = Stopwatch.GetTimestamp();

        var deletedEntitiesCount = await _executorRepository.RemoveExecutorsFollowingRetentionPolicy(
            _dateTimeProvider.NowUtc.AddDays(-retentionDays), cancellationToken);

        _logger.LogInformation("Deleted {Count} executors", deletedEntitiesCount);
        _metrics.RecordExecutionMetrics(deletedEntitiesCount, Stopwatch.GetElapsedTime(startTimestamp));
    }
}
