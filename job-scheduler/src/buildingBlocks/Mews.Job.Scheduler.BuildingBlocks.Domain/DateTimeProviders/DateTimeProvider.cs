namespace Mews.Job.Scheduler.Domain.DateTimeProviders;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime NowUtc => DateTime.UtcNow;

    public DateTime Now => DateTime.Now;
}
