namespace Mews.Job.Scheduler.BuildingBlocks.Domain;

[Serializable]
public abstract class Entity : IEntity
{
    protected Entity()
    {
    }

    public override string ToString()
    {
        return $"[ENTITY: {GetType().Name}]";
    }

    public abstract object[] GetKeys();
}

[Serializable]
public abstract class Entity<TKey> : Entity, IEntity<TKey>
    where TKey : notnull
{
    public virtual TKey Id { get; set; }

    // Enable assigning null for Entity Framework. It should not be nullable as it affects db
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    protected Entity()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
    }

    protected Entity(TKey id)
    {
        Id = id;
    }

    public override object[] GetKeys()
    {
        return new object[] { Id };
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"[ENTITY: {GetType().Name}] Id = {Id}";
    }
}
