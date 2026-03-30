namespace Mews.Job.Scheduler;

public sealed class EntityNotFoundException(string message) : Exception(message)
{
    public EntityNotFoundException(Type type, object identifier)
        : this($"Entity '{type.Name}' with identifier '{identifier}' was not found.")
    {
    }
}
