namespace Mews.Job.Scheduler.Features.Jobs.Dtos;

public sealed class ConfirmResultParametersDto
{
    public required Guid JobExecutionId { get; init; }

    public required JobExecutionResultParametersDto Parameters { get; init; }
}

public sealed record JobExecutionResultParametersDto
{
    public required JobExecutionStateDto State { get; init; }

    public required string? Tag { get; init; }

    public required bool DeleteJob { get; init; }

    public required string? FutureRunData { get; init; }
}
