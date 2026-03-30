using Mews.Job.Scheduler.BuildingBlocks.Application.CQS;
using Mews.Job.Scheduler.Domain.JobExecutions;

namespace Mews.Job.Scheduler.Core.Application.JobProcessing.Commands;

public sealed class ConfirmResultCommand : ICommand
{
    public Guid JobExecutionId { get; init; }

    public required JobExecutionState State { get; init; }

    public required string? Tag { get; init; }

    public required bool DeleteJob { get; init; }

    public required string? FutureRunData { get; init; }
}
