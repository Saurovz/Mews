using System.Linq.Expressions;
using Mews.Job.Scheduler.BuildingBlocks.Infrastructure.Repositories;
using Mews.Job.Scheduler.Core.EntityFrameworkCore.Helpers;
using Mews.Job.Scheduler.Domain.JobExecutions;
using Microsoft.EntityFrameworkCore;

namespace Mews.Job.Scheduler.Core.EntityFrameworkCore.JobExecutions;

public sealed class JobExecutionRepository : EfCoreRepository<JobSchedulerDbContext, JobExecution, Guid>, IJobExecutionRepository
{
    private const int MaxJobExecutionsBatchSize = 20;

    public JobExecutionRepository(JobSchedulerDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<List<JobExecution>> GetJobExecutionsToTimeoutAsync(List<Domain.Jobs.Job> jobs, CancellationToken cancellationToken)
    {
        return await SqlHelpers.CheckedAction(cancellationToken, async (ct) =>
            await Union(BuildTimeoutPredicates(jobs)).ToListAsync(ct)
        );
    }

    public async Task<int> RemoveJobExecutionsOlderThanAsync(DateTime retentionDateUtc, CancellationToken cancellationToken)
    {
        return await SqlHelpers.CheckedAction(cancellationToken, async ct =>
            await GetDbSet()
                .Where(e => e.StartUtc < retentionDateUtc)
                .ExecuteDeleteAsync(ct)
        );
    }

    public async Task<JobExecution?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await SqlHelpers.CheckedAction(cancellationToken, async ct =>
            await GetAsync(id, cancellationToken: ct)
        );
    }

    public async Task<JobExecution> GetRequiredByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await SqlHelpers.CheckedAction(async ct => await GetRequiredAsync(id, cancellationToken: ct), cancellationToken);
    }

    public async Task<IEnumerable<JobExecution>> GetFilteredAsync(JobExecutionFilters filters, CancellationToken cancellationToken)
    {
        return await SqlHelpers.CheckedAction(
            async ct =>
            {
                var baseQueryable = GetQueryable();
                var queryableWithPredicates = BuildFilterPredicates(filters)
                    .Aggregate(baseQueryable, (q, p) => q.Where(p))
                    .Include(e => e.Job.Executor);

                return await QueryHelpers.ApplyLimitation(queryableWithPredicates, e => e.StartUtc, filters.Limitation, orderByDescending: true).ToListAsync(ct);
            },
            cancellationToken
        );
    }

    public async Task<JobExecution?> GetByJobIdWithTransactionIdentifier(Guid jobId, string transactionIdentifier, CancellationToken cancellationToken)
    {
        return await SqlHelpers.CheckedAction(
            async ct =>
            {
                var executions = await GetListAsync(execution => execution.JobId == jobId && execution.TransactionIdentifier == transactionIdentifier, cancellationToken: ct);

                return executions.SingleOrDefault();
            },
            cancellationToken
        );
    }

    public async Task<List<JobExecution>> GetByIdsAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken)
    {
        return await SqlHelpers.CheckedAction(cancellationToken, async ct =>
            await GetListAsync(predicate: execution => ids.Contains(execution.Id), cancellationToken: ct)
        );
    }

    private static List<Expression<Func<JobExecution, bool>>> BuildTimeoutPredicates(List<Domain.Jobs.Job> jobs)
    {
        return jobs.Take(MaxJobExecutionsBatchSize).Select(BuildTimeoutPredicate).ToList();
    }

    private static Expression<Func<JobExecution, bool>> BuildTimeoutPredicate(Domain.Jobs.Job job)
    {
        return execution => (
            execution.JobId == job.Id &&
            execution.State == JobExecutionState.InProgress &&
            execution.IsDeleted == false
        );
    }

    private IQueryable<JobExecution> Union(List<Expression<Func<JobExecution, bool>>> predicates)
    {
        if (predicates.Count < 1)
        {
            return GetDbSet().Where(e => false);
        }

        var query = GetDbSet().Where(predicates.First());
        if (predicates.Count == 1)
        {
            return query;
        }

        return query.Union(Union(predicates.Skip(1).ToList()));
    }

    private static IList<Expression<Func<JobExecution, bool>>> BuildFilterPredicates(JobExecutionFilters filters)
    {
        var predicates = new List<Expression<Func<JobExecution, bool>>>();
        if (filters.Ids != null)
        {
            predicates.Add(e => filters.Ids.Contains(e.Id));
        }
        if (filters.JobIds != null)
        {
            predicates.Add(e => EF.Constant(filters.JobIds).Contains(e.JobId));
        }
        if (filters.StartInterval is not null)
        {
            var interval = filters.StartInterval;
            if (interval.StartUtc is not null)
            {
                predicates.Add(e => interval.StartUtc <= e.StartUtc);
            }
            if (interval.EndUtc is not null)
            {
                predicates.Add(e => e.StartUtc < interval.EndUtc);
            }
        }
        if (filters.ExecutorTypeNames is not null)
        {
            predicates.Add(e => EF.Constant(filters.ExecutorTypeNames).Contains(e.ExecutorTypeNameValue));
        }
        if (filters.States is not null)
        {
            var allStates = Enum.GetValues<JobExecutionStates>();
            var setStates = allStates.Where(s => filters.States.Value.HasFlag(s));
            var predicateStates = setStates.Select(s => ((long)Math.Log(s.ToInt64(), 2)).ToEnum<JobExecutionState>()).ToList();
            predicates.Add(e => EF.Constant(predicateStates).Contains(e.State));
        }
        if (!filters.ShowDeleted)
        {
            predicates.Add(j => j.IsDeleted == false);
        }

        return predicates;
    }
}
