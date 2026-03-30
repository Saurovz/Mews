using System.Diagnostics;
using Mews.Job.Scheduler.BuildingBlocks.Types.Extensions;
using Mews.Job.Scheduler.Core.EntityFrameworkCore.Observability;
using Mews.Job.Scheduler.Domain;
using Mews.Job.Scheduler.Domain.JobExecutions;
using Mews.Job.Scheduler.Domain.Jobs;

namespace Mews.Job.Scheduler.Core.EntityFrameworkCore.JobPersistence;

public sealed class JobPersistence : IJobPersistence
{
    private readonly JobSchedulerDbContext _context;

    public JobPersistence(JobSchedulerDbContext context, IJobRepository jobRepository, IJobExecutionRepository jobExecutionRepository)
    {
        _context = context;
        Jobs = jobRepository;
        JobExecutions = jobExecutionRepository;
    }

    public IJobRepository Jobs { get; }

    public IJobExecutionRepository JobExecutions { get; }

    public async Task AddRangeAsync(IReadOnlyList<Domain.Jobs.Job> job, CancellationToken cancellationToken)
    {
        await _context.Jobs.AddRangeAsync(job, cancellationToken);
    }

    public async Task AddAsync(JobExecution jobExecution, CancellationToken cancellationToken)
    {
        await _context.JobExecutions.AddAsync(jobExecution, cancellationToken);
    }
    
    public async Task AddRangeAsync(IReadOnlyList<JobExecution> jobExecutions, CancellationToken cancellationToken)
    {
        await _context.JobExecutions.AddRangeAsync(jobExecutions, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
