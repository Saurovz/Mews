namespace Mews.Job.Scheduler.Features.Jobs.Dtos;

public sealed class JobExecutorMetadataDto
{
    public required string Type { get; init; }

    public required string Team { get; init; }
}
