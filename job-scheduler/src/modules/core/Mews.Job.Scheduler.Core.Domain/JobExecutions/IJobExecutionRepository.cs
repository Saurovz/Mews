namespace Mews.Job.Scheduler.Domain.JobExecutions;

public interface IJobExecutionRepository
{
    Task<List<JobExecution>> GetJobExecutionsToTimeoutAsync(List<Domain.Jobs.Job> jobs, CancellationToken cancellationToken);

    Task<JobExecution?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<JobExecution> GetRequiredByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IEnumerable<JobExecution>> GetFilteredAsync(JobExecutionFilters filters, CancellationToken cancellationToken);

    Task<JobExecution?> GetByJobIdWithTransactionIdentifier(Guid jobId, string transactionIdentifier, CancellationToken cancellationToken);

    Task<List<JobExecution>> GetByIdsAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken);

    public Task<int> RemoveJobExecutionsOlderThanAsync(DateTime retentionDateUtc, CancellationToken cancellationToken);
}
