using System.Collections.Immutable;
using System.Linq.Expressions;
using Mews.Job.Scheduler.BuildingBlocks.Infrastructure.Repositories;
using Mews.Job.Scheduler.BuildingBlocks.Types.DateTimeSpan;
using Mews.Job.Scheduler.Core.EntityFrameworkCore.Helpers;
using Mews.Job.Scheduler.Domain.Jobs;
using Microsoft.EntityFrameworkCore;

namespace Mews.Job.Scheduler.Core.EntityFrameworkCore.Jobs;

public sealed class JobRepository : EfCoreRepository<JobSchedulerDbContext, Domain.Jobs.Job, Guid>, IJobRepository
{
    /// <summary>
    /// Based on the ServiceBus TTL of a message, it makes no sense to attempt a re-schedule before this delay elapses.
    /// </summary>
    private const double RescheduleDelayInMinutes = -3;

    public JobRepository(JobSchedulerDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<Domain.Jobs.Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await SqlHelpers.CheckedAction(cancellationToken, async ct =>
            await GetAsync(id, cancellationToken: ct)
        );
    }

    public async Task<IReadOnlyList<Domain.Jobs.Job>> GetFilteredAsync(JobFilters filters, CancellationToken cancellationToken)
    {
        return await SqlHelpers.CheckedAction(
            async ct =>
            {
                var predicates = BuildFilterPredicates(filters);
                var baseQueryable = GetQueryable();
                var queryableWithPredicates = predicates
                    .Aggregate(baseQueryable, (q, p) => q.Where(p))
                    .Include(j => j.Executor);
                var limitedQueryable = filters.Limitation is not null
                    ? QueryHelpers.ApplyLimitation(queryableWithPredicates, j => j.StartUtc, filters.Limitation)
                    : queryableWithPredicates;
                return await limitedQueryable.ToListAsync(ct);
            },
            cancellationToken
        );
    }

    public async Task<Domain.Jobs.Job> GetRequiredByIdAsync(Guid id, CancellationToken cancellationToken, bool includeExecutor = false)
    {
        var result = await SqlHelpers.CheckedAction(
            async ct =>
            {
                var queryable = GetDbSet().Where(q => q.Id == id);

                if (includeExecutor)
                {
                    queryable = queryable.Include(j => j.Executor);
                }

                return await queryable.FirstOrDefaultAsync(ct);
            },
            cancellationToken
        );
    
        return result ?? throw new EntityNotFoundException(typeof(Domain.Jobs.Job), id); 
    }

    public async Task<List<Domain.Jobs.Job>> GetByIdsAsync(IReadOnlyList<Guid> ids, bool includeExecutor, CancellationToken cancellationToken)
    {
        return await SqlHelpers.CheckedAction(
            async ct =>
            {
                var queryable = GetDbSet().Where(j => ids.Contains(j.Id));
                return includeExecutor
                    ? await queryable.Include(j => j.Executor).ToListAsync(ct)
                    : await queryable.ToListAsync(ct);
            },
            cancellationToken
        );
    }

    public async Task<List<Domain.Jobs.Job>> GetJobsToTimeoutAsync(DateTime nowUtc, CancellationToken cancellationToken)
    {
        var pendingJobs = await SqlHelpers.CheckedAction(cancellationToken, async ct =>
        {
            return await GetDbSet()
                .Where(BuildTimeoutPredicate())
                .Include(j => j.Executor)
                .ToListAsync(ct);
        });

        return pendingJobs.Where(j => j.ExecutionStartUtc?.Add(j.MaxExecutionTime) < nowUtc).ToList();
    }

    public async Task<List<Domain.Jobs.Job>> GetNextBatchToScheduleAsync(DateTime endUtc, CancellationToken cancellationToken)
    {
        var batch = await SqlHelpers.CheckedAction(
            async ct =>
                await GetDbSet()
                    .Where(BuildNextBatchPredicate(endUtc))
                    .Include(j => j.Executor)
                    .ToListAsync(ct),
            cancellationToken
        );

        return batch.OrderBy(j => j.StartUtc).ToList();
    }

    public async Task<IReadOnlyList<Domain.Jobs.Job>> GetUnregisteredJobs(IImmutableSet<string> unregisteredExecutors, CancellationToken cancellationToken)
    {
        return await SqlHelpers.CheckedAction(cancellationToken, async ct =>
            await GetListAsync(predicate: BuildUnregisteredJobsPredicate(unregisteredExecutors), cancellationToken: ct)
        );
    }

    public async Task<int> RemoveJobsFollowingRetentionPolicy(DateTime retentionDateUtc, CancellationToken cancellationToken)
    {
        return await SqlHelpers.CheckedAction(cancellationToken, async ct =>
            await GetDbSet()
                .Where(j => j.IsDeleted && j.StartUtc <= retentionDateUtc && j.JobExecutions.Count == 0)
                .ExecuteDeleteAsync(ct)
        );
    }

    private static IList<Expression<Func<Domain.Jobs.Job, bool>>> BuildFilterPredicates(JobFilters filters)
    {
        var predicates = new List<Expression<Func<Domain.Jobs.Job, bool>>>();
        if (filters.Ids is not null)
        {
            predicates.Add(j => filters.Ids.Contains(j.Id));
        }
        if (filters.Name is not null)
        {
            predicates.Add(j => j.NameNew != null && j.NameNew.Contains(filters.Name));
        }
        if (filters.ExecutorTypeNames is not null)
        {
            predicates.Add(j => filters.ExecutorTypeNames.Contains(j.Executor.Type));
        }
        if (filters.States is not null)
        {
            var allStates = Enum.GetValues<JobStates>();
            var setStates = allStates.Where(s => filters.States.Value.HasFlag(s));
            var predicateStates = setStates.Select(s => ((long)Math.Log(s.ToInt64(), 2)).ToEnum<JobState>());
            predicates.Add(j => predicateStates.Contains(j.State));
        }
        if (filters.StartUtc is not null)
        {
            predicates.Add(j => filters.StartUtc < j.StartUtc);
        }
        if (filters.EndUtc is not null)
        {
            predicates.Add(j => j.StartUtc < filters.EndUtc);
        }
        if (!filters.ShowDeleted)
        {
            predicates.Add(j => j.IsDeleted == false);
        }

        return predicates;
    }

    private static Expression<Func<Domain.Jobs.Job, bool>> BuildTimeoutPredicate()
    {
        return job => (
            job.State == JobState.InProgress &&
            job.IsDeleted == false
        );
    }

    private static Expression<Func<Domain.Jobs.Job, bool>> BuildNextBatchPredicate(DateTime endUtc)
    {
        var rescheduleUtc = endUtc.AddMinutes(RescheduleDelayInMinutes);
        return j => (
            (
                j.State == JobState.Pending &&
                j.StartUtc < endUtc &&
                j.IsDeleted == false
            ) ||
            (
                j.State == JobState.Scheduled &&
                j.ScheduledUtc < rescheduleUtc &&
                j.IsDeleted == false
            )
        );
    }

    private static Expression<Func<Domain.Jobs.Job, bool>> BuildUnregisteredJobsPredicate(IImmutableSet<string> unregisteredExecutors)
    {
        return j => (
            unregisteredExecutors.Contains(j.Executor.Type) &&
            j.IsDeleted == false
        );
    }
}
