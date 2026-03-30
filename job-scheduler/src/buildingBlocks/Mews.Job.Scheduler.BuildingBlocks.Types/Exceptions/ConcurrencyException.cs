namespace Mews.Job.Scheduler;

public sealed class ConcurrencyException : Exception, IDetailedException
{
    public ConcurrencyException(Exception innerException, object debugDetails)
        : base(
            message: "Conflicting operation is being performed at this time. Please try again in a few seconds.",
            innerException: innerException
        )
    {
        DebugDetails = debugDetails;
    }

    public object DebugDetails { get; }
}
