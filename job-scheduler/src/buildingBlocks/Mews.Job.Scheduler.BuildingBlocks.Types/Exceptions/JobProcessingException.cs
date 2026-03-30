namespace Mews.Job.Scheduler;

public sealed class JobProcessingException(JobSchedulerServiceExceptionReason reason, Exception innerException)
    : JobSchedulerServiceException(reason, "An error occurred during job processing.", innerException);
