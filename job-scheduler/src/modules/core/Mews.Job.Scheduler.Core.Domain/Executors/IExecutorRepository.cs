namespace Mews.Job.Scheduler.Domain.Executors;

public interface IExecutorRepository
{
    Task<IReadOnlyList<Executor>> GetExecutorsAsync(CancellationToken cancellationToken);

    Task<Executor> GetRequiredExecutorByTypeAsync(string type, CancellationToken cancellationToken);

    Task<IReadOnlyList<Executor>> GetExecutorByTypesAsync(IReadOnlyList<string> executorTypes, CancellationToken cancellationToken);

    Task<IReadOnlyList<Executor>> GetByIdsAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken);

    Task AddRangeAsync(IReadOnlyList<Executor> executors, CancellationToken cancellationToken);

    Task<int> RemoveExecutorsFollowingRetentionPolicy(DateTime retentionDateUtc, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
