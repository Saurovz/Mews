namespace Mews.Job.Scheduler.Domain.DateTimeProviders;

public interface IDateTimeProvider
{
    DateTime NowUtc { get; }
    
    DateTime Now { get; }
}
