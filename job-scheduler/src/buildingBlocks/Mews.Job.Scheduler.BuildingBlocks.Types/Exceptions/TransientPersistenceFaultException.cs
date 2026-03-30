namespace Mews.Job.Scheduler;

public sealed class TransientPersistenceFaultException(Exception innerException)
    : Exception(default, innerException);
