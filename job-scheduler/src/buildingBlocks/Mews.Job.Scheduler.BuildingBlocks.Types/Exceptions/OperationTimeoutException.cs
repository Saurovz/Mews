namespace Mews.Job.Scheduler;

public sealed class OperationTimeoutException(Exception? innerException = null)
    : Exception("Operation has timed out.", innerException);
