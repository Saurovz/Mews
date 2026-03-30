using System.Collections.Immutable;

namespace Mews.Job.Scheduler.Domain.Jobs;

public interface IJobRepository
{
    Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Job>> GetFilteredAsync(JobFilters filters, CancellationToken cancellationToken);

    Task<Domain.Jobs.Job> GetRequiredByIdAsync(Guid id, CancellationToken cancellationToken, bool includeExecutor = false);

    Task<List<Job>> GetByIdsAsync(IReadOnlyList<Guid> ids, bool includeExecutor, CancellationToken cancellationToken);

    Task<List<Job>> GetJobsToTimeoutAsync(DateTime nowUtc, CancellationToken cancellationToken);

    Task<List<Job>> GetNextBatchToScheduleAsync(DateTime endUtc, CancellationToken cancellationToken);

    Task<IReadOnlyList<Job>> GetUnregisteredJobs(IImmutableSet<string> unregisteredExecutors, CancellationToken cancellationToken);

    Task<int> RemoveJobsFollowingRetentionPolicy(DateTime addDays, CancellationToken cancellationToken);
}
