using Mews.Job.Scheduler.BuildingBlocks.Application.CQS;
using Mews.Job.Scheduler.Domain.Jobs;

namespace Mews.Job.Scheduler.Core.Application.Jobs.Commands;

public sealed class JobGetFilteredCommand : ICommand<IEnumerable<Domain.Jobs.Job>>
{
    public required JobFilters Filters { get; init; }
}
