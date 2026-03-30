using Mews.Job.Scheduler.BuildingBlocks.Domain;

namespace Mews.Job.Scheduler.BuildingBlocks.Infrastructure.Repositories;

public interface IEfCoreRepository<TEntity> : IRepository<TEntity>
    where TEntity : class, IEntity
{
}

public interface IEfCoreRepository<TEntity, TKey> : IEfCoreRepository<TEntity>, IRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : notnull
{
}
