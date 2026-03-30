namespace Mews.Job.Scheduler.Domain.Executors;

/// <summary>
/// Represents the result of synchronizing executors.
/// </summary>
/// <param name="AddedExecutors">Newly added executors</param>
/// <param name="RemovedExecutors">Executors that were removed</param>
/// <param name="Executors">All synchronized executors</param>
public sealed record SynchronizedExecutors(
    IReadOnlyList<Executor> AddedExecutors,
    IReadOnlyList<Executor> RemovedExecutors,
    IReadOnlyList<Executor> Executors);
