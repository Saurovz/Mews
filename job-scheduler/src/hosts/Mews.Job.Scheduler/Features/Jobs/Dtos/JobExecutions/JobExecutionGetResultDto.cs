namespace Mews.Job.Scheduler.Features.Jobs.Dtos;

public sealed class JobExecutionGetResultDto
{
    public required IEnumerable<JobExecutionDto> JobExecutions { get; init; }
}
