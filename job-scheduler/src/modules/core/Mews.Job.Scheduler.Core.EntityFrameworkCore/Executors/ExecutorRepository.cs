using Mews.Job.Scheduler.BuildingBlocks.Infrastructure.Repositories;
using Mews.Job.Scheduler.Core.EntityFrameworkCore.Helpers;
using Mews.Job.Scheduler.Domain.Executors;
using Microsoft.EntityFrameworkCore;

namespace Mews.Job.Scheduler.Core.EntityFrameworkCore.Executors;

public sealed class ExecutorRepository : EfCoreRepository<JobSchedulerDbContext, Executor, Guid>, IExecutorRepository
{
    public ExecutorRepository(JobSchedulerDbContext dbContext) 
        : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<Executor>> GetExecutorsAsync(CancellationToken cancellationToken)
    {
        return await SqlHelpers.CheckedAction(cancellationToken, GetListAsync);
    }

    public async Task<Executor> GetRequiredExecutorByTypeAsync(string type, CancellationToken cancellationToken)
    {
        return await SqlHelpers.CheckedAction(cancellationToken, async ct =>
        {
            var executor = await GetDbSet().FirstOrDefaultAsync(e => e.Type == type, ct);
            return executor ?? throw new EntityNotFoundException(typeof(Executor), type);
        });
    }

    public async Task<IReadOnlyList<Executor>> GetExecutorByTypesAsync(IReadOnlyList<string> executorTypes, CancellationToken cancellationToken)
    {
        return await SqlHelpers.CheckedAction(ct => GetDbSet().Where(e => executorTypes.Contains(e.Type)).ToListAsync(ct), cancellationToken);
    }

    public async Task<IReadOnlyList<Executor>> GetByIdsAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken)
    {
        return await SqlHelpers.CheckedAction(cancellationToken, async ct =>
            await GetListAsync(predicate: e => ids.Contains(e.Id), cancellationToken: ct)
        );
    }

    public async Task AddRangeAsync(IReadOnlyList<Executor> executors, CancellationToken cancellationToken)
    {
        await DbContext.AddRangeAsync(executors, cancellationToken); 
    }

    public async Task<int> RemoveExecutorsFollowingRetentionPolicy(DateTime retentionDateUtc, CancellationToken cancellationToken)
    {
        return await SqlHelpers.CheckedAction(cancellationToken, async ct =>
            await GetDbSet()
                .Where(e => e.DeletedUtc != null && e.DeletedUtc < retentionDateUtc && e.Jobs.Count == 0)
                .ExecuteDeleteAsync(ct)
        );
    }

    public new async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await base.SaveChangesAsync(cancellationToken);
    }
}
