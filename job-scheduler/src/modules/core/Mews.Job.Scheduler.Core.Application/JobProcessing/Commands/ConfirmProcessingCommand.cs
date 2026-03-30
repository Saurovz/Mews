using Mews.Job.Scheduler.BuildingBlocks.Application.CQS;

namespace Mews.Job.Scheduler.Core.Application.JobProcessing.Commands;

public sealed class ConfirmProcessingCommand : ICommand<Domain.JobExecutions.JobExecution>
{
    public required Guid JobId { get; init; }
    
    public required string ExecutionTransactionIdentifier { get; init; }
}
