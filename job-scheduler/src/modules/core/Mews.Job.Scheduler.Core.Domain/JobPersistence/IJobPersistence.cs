using Mews.Job.Scheduler.Domain.JobExecutions;
using Mews.Job.Scheduler.Domain.Jobs;

namespace Mews.Job.Scheduler.Domain;

public interface IJobPersistence
{
    IJobRepository Jobs { get; }

    IJobExecutionRepository JobExecutions { get; }

    Task AddRangeAsync(IReadOnlyList<Domain.Jobs.Job> job, CancellationToken cancellationToken);
    
    Task AddAsync(JobExecution jobExecution, CancellationToken cancellationToken);
    
    Task AddRangeAsync(IReadOnlyList<JobExecution> jobExecutions, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
