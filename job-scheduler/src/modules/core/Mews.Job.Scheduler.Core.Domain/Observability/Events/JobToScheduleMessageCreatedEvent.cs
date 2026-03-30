namespace Mews.Job.Scheduler.Observability.Events;

public sealed record JobToScheduleMessageCreatedEvent(Guid JobId, string JobFullName);
