namespace Mews.Job.Scheduler;

public class JobSchedulerServiceException : Exception
{
    public JobSchedulerServiceException(JobSchedulerServiceExceptionReason reason, Exception innerException)
        : this(reason, "An error occured while processing the request.", innerException)
    {
    }
    
    protected JobSchedulerServiceException(JobSchedulerServiceExceptionReason reason, string message, Exception innerException)
        : base(message, innerException)
    {
        Reason = reason;
    }
    
    public JobSchedulerServiceExceptionReason Reason { get; }

    public override string Message => GetMessage();

    private string GetMessage()
    {
        return Reason switch
        {
            JobSchedulerServiceExceptionReason.InternalError => $"{base.Message}",
            JobSchedulerServiceExceptionReason.ServiceCommunicationProblem => $"{base.Message}",
            _ => $"{base.Message} {InnerException!.Message}"
        };
    }
}

public enum JobSchedulerServiceExceptionReason
{
    /// <summary>
    /// Resource cannot be found.
    /// </summary>
    EntityNotFound,
    
    /// <summary>
    /// Resource attempted to transition to an invalid state.
    /// </summary>
    EntityInvalidStateTransition,
    
    /// <summary>
    /// Indicates that a conflict occurred during resource processing,
    /// such as when the entity is already being processed.
    /// </summary>
    EntityProcessingConflict,
    
    /// <summary>
    /// A general error occured while communicating with external services (e.g. Database).
    /// </summary>
    ServiceCommunicationProblem,
    
    /// <summary>
    /// An internal error occurred while processing the request.
    /// </summary>
    InternalError
}
