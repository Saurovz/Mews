namespace Mews.Job.Scheduler.Job;

public sealed class EntityProcessingException(Guid jobId, string name)
    : Exception(message: $"Job '{name}' with identifier '{jobId}' is already in progress or has been processed.");
