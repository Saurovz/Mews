using Mews.Atlas.Aspire.Testing.Components.SqlServer;
using Mews.Job.Scheduler.Core.EntityFrameworkCore;
using Mews.Job.Scheduler.Domain.Executors;
using Mews.Job.Scheduler.Domain.JobExecutions;
using Mews.Job.Scheduler.Domain.Profiles;

namespace Mews.Job.Scheduler.Aspire.IntegrationTests.Helpers;

public static class SampleData
{
    public static async Task<Domain.Jobs.Job> AddJobAsync(
        DbContextFactory<JobSchedulerDbContext> dbContextFactory,
        Func<SystemProfile, Domain.Jobs.Job> create,
        CancellationToken cancellationToken)
    {
        var result = await AddJobRangeAsync(
            dbContextFactory,
            profile => new List<Domain.Jobs.Job> { create(profile) },
            cancellationToken
        );
        return result.Single();
    }

    public static async Task<IReadOnlyList<Domain.Jobs.Job>> AddJobRangeAsync(
        DbContextFactory<JobSchedulerDbContext> dbContextFactory,
        Func<SystemProfile, IReadOnlyList<Domain.Jobs.Job>> create,
        CancellationToken cancellationToken)
    {
        var profile = new SystemProfile();
        await using var dbContext = dbContextFactory.CreateDbContext();
        var arrangedJobs = create(profile);
        dbContext.Jobs.AddRange(arrangedJobs);
        await dbContext.SaveChangesAsync(cancellationToken);
        return arrangedJobs;
    }

    public static async Task<JobExecution> AddJobExecutionAsync(
        DbContextFactory<JobSchedulerDbContext> dbContextFactory,
        Func<SystemProfile, JobExecution> create,
        CancellationToken cancellationToken)
    {
        var result = await AddJobExecutionRangeAsync(
            dbContextFactory,
            p => new List<JobExecution> { create(p) },
            cancellationToken
        );
        return result.Single();
    }

    public static async Task<IReadOnlyList<JobExecution>> AddJobExecutionRangeAsync(
        DbContextFactory<JobSchedulerDbContext> dbContextFactory,
        Func<SystemProfile, IReadOnlyList<JobExecution>> create,
        CancellationToken cancellationToken)
    {
        var profile = new SystemProfile();
        var arrangedJobExecutions = create(profile);
        await using var dbContext = dbContextFactory.CreateDbContext();
        await dbContext.JobExecutions.AddRangeAsync(arrangedJobExecutions, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return arrangedJobExecutions;
    }

    public static async Task<Executor> AddExecutorAsync(
        DbContextFactory<JobSchedulerDbContext> dbContextFactory,
        Func<Executor> create,
        CancellationToken cancellationToken)
    {
        var executors = await AddExecutorRangeAsync(
            dbContextFactory,
            () => [create()],
            cancellationToken
        );

        return executors.Single();
    }

    public static async Task<IReadOnlyList<Executor>> AddExecutorRangeAsync(
        DbContextFactory<JobSchedulerDbContext> dbContextFactory,
        Func<IReadOnlyList<Executor>> create,
        CancellationToken cancellationToken)
    {
        var arrangedExecutors = create();
        await using var dbContext = dbContextFactory.CreateDbContext();
        await dbContext.AddRangeAsync(arrangedExecutors, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return arrangedExecutors;
    }
}
