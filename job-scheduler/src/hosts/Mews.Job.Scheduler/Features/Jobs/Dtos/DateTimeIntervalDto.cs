namespace Mews.Job.Scheduler.Features.Jobs.Dtos;

public sealed class DateTimeIntervalDto
{
    public DateTime? StartUtc { get; init; }
    
    public DateTime? EndUtc { get; init; }
}
