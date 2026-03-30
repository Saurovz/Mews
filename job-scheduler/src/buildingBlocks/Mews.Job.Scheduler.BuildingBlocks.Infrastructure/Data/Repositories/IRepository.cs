using System.Linq.Expressions;
using Mews.Job.Scheduler.BuildingBlocks.Domain;

namespace Mews.Job.Scheduler.BuildingBlocks.Infrastructure.Repositories;

public interface IRepository
{
}

public interface IRepository<TEntity> : IRepository
    where TEntity : class, IEntity
{
    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    public Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    public Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<List<TEntity>> GetListAsync(CancellationToken cancellationToken = default);

    Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
}

public interface IRepository<TEntity, TKey> : IRepository<TEntity>
    where TEntity : class, IEntity<TKey>
    where TKey : notnull
{
    Task<TEntity?> GetAsync(TKey id, CancellationToken cancellationToken = default);

    Task<List<TEntity>> GetAsync(IReadOnlyCollection<TKey> ids, CancellationToken cancellationToken = default);

    Task<List<TEntity>> GetAsync(IReadOnlySet<TKey> ids, CancellationToken cancellationToken = default);

    Task RemoveAsync(TKey id, CancellationToken cancellationToken = default);
}
