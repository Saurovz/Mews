namespace Mews.Job.Scheduler;

public interface IDetailedException
{
    /// <summary>
    /// Object primarily used for logging or other forms of debugging, not to be used by business logic.
    /// </summary>
    public object DebugDetails { get; }
}
