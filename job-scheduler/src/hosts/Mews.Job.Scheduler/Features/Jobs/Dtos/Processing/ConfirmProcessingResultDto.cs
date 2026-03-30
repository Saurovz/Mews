namespace Mews.Job.Scheduler.Features.Jobs.Dtos;

public sealed class ConfirmProcessingResultDto
{
    public required Guid JobExecutionId { get; init; }
    
    public required DateTime ExecutionStartUtc { get; init; }
}
