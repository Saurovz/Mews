namespace Mews.Job.Scheduler.BuildingBlocks.Domain;

public interface IEntity
{
    object[] GetKeys();
}

public interface IEntity<TKey> : IEntity
    where TKey : notnull
{
    /// <summary>
    /// Unique identifier for this entity.
    /// </summary>
    TKey Id { get; }
}


public interface IHaveCreationTime
{
    /// <summary>
    /// The time this entity was created.
    /// </summary>
    DateTime CreatedUtc { get; }
}

public interface IHaveModificationTime
{
    /// <summary>
    /// The time this entity was last updated.
    /// </summary>
    DateTime UpdatedUtc { get; }
}

public interface IHaveCreationAndModificationTime : IHaveCreationTime, IHaveModificationTime
{
}
