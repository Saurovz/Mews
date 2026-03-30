using Mews.Job.Scheduler.BuildingBlocks.Application.CQS;
using Mews.Job.Scheduler.Domain.JobExecutions;

namespace Mews.Job.Scheduler.Core.Application.JobExecutions.Commands;

public class JobExecutionGetFilteredCommand : ICommand<IEnumerable<JobExecution>>
{
    public required JobExecutionFilters Filters { get; init; }
}
