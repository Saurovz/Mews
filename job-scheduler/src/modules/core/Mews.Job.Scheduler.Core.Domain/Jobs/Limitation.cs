using System.Linq.Expressions;
using Mews.Job.Scheduler.BuildingBlocks.Domain;

namespace Mews.Job.Scheduler.Domain.Jobs;

public class Limitation
{
    public required ushort Count { get; init; }

    public required ushort StartIndex { get; init; }
}

public sealed class Limitation<TEntity> : Limitation
    where TEntity : Entity
{
    public required EagerLoad<TEntity> EagerLoad { get; init; }
}

public sealed class EagerLoad<TEntity>
    where TEntity : Entity
{
    public required IReadOnlyList<Expression<Func<TEntity, object>>> Selectors { get; init; }
}
