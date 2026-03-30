namespace Mews.Job.Scheduler.Features.Jobs.Dtos;

public sealed class ConfirmProcessingParametersDto
{
    public required Guid JobId { get; init; }
    
    public required string TransactionIdentifier { get; init; }
}
