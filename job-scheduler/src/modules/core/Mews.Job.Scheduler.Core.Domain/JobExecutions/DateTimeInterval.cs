namespace Mews.Job.Scheduler.Domain.JobExecutions;

public sealed class DateTimeInterval
{
    public DateTimeInterval(DateTime? startUtc, DateTime? endUtc)
    {
        if (startUtc == null && endUtc == null)
        {
            throw new ArgumentException("At least one of the dates must be provided.");
        }
        
        if (startUtc != null && endUtc != null && endUtc <= startUtc)
        {
            throw new ArgumentException("End date must be greater than start date.");
        }

        StartUtc = startUtc;
        EndUtc = endUtc;
    }

    public DateTime? StartUtc { get; }

    public DateTime? EndUtc { get; }
}
