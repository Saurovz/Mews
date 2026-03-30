namespace Mews.Job.Scheduler.Features.Jobs.Dtos;

public sealed class LimitationDto
{
    public required ushort Count { get; init; }

    public required ushort StartIndex { get; init; }
}
