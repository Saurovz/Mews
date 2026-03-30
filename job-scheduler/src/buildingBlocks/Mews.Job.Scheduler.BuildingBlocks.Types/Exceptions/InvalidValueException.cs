namespace Mews.Job.Scheduler;

public sealed class InvalidValueException(Exception innerException)
    : Exception("Invalid value.", innerException: innerException);
