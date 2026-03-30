using System.Linq.Expressions;
using Mews.Job.Scheduler.BuildingBlocks.Domain;
using Mews.Job.Scheduler.Domain.Jobs;
using Microsoft.EntityFrameworkCore;

namespace Mews.Job.Scheduler.Core.EntityFrameworkCore.Helpers;

public static class QueryHelpers
{

    public static IQueryable<TEntity> ApplyLimitation<TEntity, TKey>(IQueryable<TEntity> query, Expression<Func<TEntity, TKey>> orderBy, Limitation limitation, bool orderByDescending = false)
        where TEntity : Entity<Guid>
    {
        return ApplyOrdering(query, orderBy, orderByDescending).Skip(limitation.StartIndex).Take(limitation.Count);
    }

    public static IQueryable<TEntity> ApplyLimitation<TEntity, TKey>(IQueryable<TEntity> query, Expression<Func<TEntity, TKey>> orderBy, Limitation<TEntity> limitation, bool orderByDescending = false)
        where TEntity : Entity<Guid>
    {
        var queryable = ApplyLimitation(query, orderBy, limitation as Limitation, orderByDescending);

        foreach (var load in limitation.EagerLoad.Selectors)
        {
            queryable = queryable.Include(load);
        }

        return queryable;
    }

    private static IQueryable<TEntity> ApplyOrdering<TEntity, TKey>(IQueryable<TEntity> query, Expression<Func<TEntity, TKey>> orderBy, bool orderByDescending)
        where TEntity : Entity<Guid>
    {
        return orderByDescending
            ? query.OrderByDescending(orderBy)
            : query.OrderBy(orderBy);
    }
}
