using Mews.Job.Scheduler.Domain.Jobs;

namespace Mews.Job.Scheduler.Domain.JobExecutions;

public sealed class JobExecutionFilters
{
    public IEnumerable<Guid>? Ids { get; init; }

    public IEnumerable<Guid>? JobIds { get; init; }

    public JobExecutionStates? States { get; init; }

    public DateTimeInterval? StartInterval { get; init; }

    public IEnumerable<string>? ExecutorTypeNames { get; init; }
    
    public bool ShowDeleted { get; init; }

    public required Limitation<JobExecution> Limitation { get; init; }
}
