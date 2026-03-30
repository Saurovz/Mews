namespace Mews.Job.Scheduler;

public sealed class UniquenessException(string propertyName, Exception innerException)
    : Exception($"Duplicate entry of {propertyName}.", innerException: innerException);
