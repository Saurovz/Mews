using System.Linq.Expressions;
using Mews.Job.Scheduler.BuildingBlocks.Domain;
using Microsoft.EntityFrameworkCore;

namespace Mews.Job.Scheduler.BuildingBlocks.Infrastructure.Repositories;

public class EfCoreRepository<TDbContext, TEntity> :
    IEfCoreRepository<TEntity>
    where TDbContext : DbContext
    where TEntity : class, IEntity
{
    protected EfCoreRepository(TDbContext dbContext)
    {
        DbContext = dbContext;
    }

    protected TDbContext DbContext { get; }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await DbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        await DbContext.Set<TEntity>().AddRangeAsync(entities, cancellationToken);
    }

    public Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        DbContext.Attach(entity);
        var updatedEntity = DbContext.Update(entity).Entity;
        return Task.FromResult(updatedEntity);
    }

    public Task RemoveAsync(TEntity entity, CancellationToken cancellationToken)
    {
        DbContext.Set<TEntity>().Remove(entity);
        return Task.CompletedTask;
    }

    public Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        DbContext.Set<TEntity>().RemoveRange(entities);
        return Task.CompletedTask;
    }

    public Task<List<TEntity>> GetListAsync(CancellationToken cancellationToken)
    {
        return GetDbSet().ToListAsync(cancellationToken);
    }

    public Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return GetDbSet().Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await DbContext.SaveChangesAsync(cancellationToken);
    }

    protected IQueryable<TEntity> GetQueryable()
    {
        return GetDbSet().AsQueryable();
    }

    protected DbSet<TEntity> GetDbSet()
    {
        return DbContext.Set<TEntity>();

    }
}

public class EfCoreRepository<TDbContext, TEntity, TKey> : EfCoreRepository<TDbContext, TEntity>,
    IEfCoreRepository<TEntity, TKey>
    where TDbContext : DbContext
    where TEntity : class, IEntity<TKey>
    where TKey : notnull
{

    protected EfCoreRepository(TDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<TEntity?> GetAsync(TKey id, CancellationToken cancellationToken)
    {
        return await GetDbSet().FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<TEntity> GetRequiredAsync(TKey id, CancellationToken cancellationToken)
    {
        return await GetAsync(id, cancellationToken) ?? throw new EntityNotFoundException(typeof(TEntity), id);
    }

    public async Task<List<TEntity>> GetAsync(IReadOnlyCollection<TKey> ids, CancellationToken cancellationToken)
    {
        var hashSet = ids.ToHashSet();
        return await GetAsync(hashSet, cancellationToken);
    }

    public async Task<List<TEntity>> GetAsync(IReadOnlySet<TKey> ids, CancellationToken cancellationToken)
    {
        return await GetListAsync(e => ids.Contains(e.Id), cancellationToken);
    }

    public async Task RemoveAsync(TKey id, CancellationToken cancellationToken)
    {
        var entity = await DbContext.Set<TEntity>().FindAsync(new object[] { id }, cancellationToken) ?? throw new EntityNotFoundException(typeof(TEntity), id);

        DbContext.Set<TEntity>().Remove(entity);
    }
}
