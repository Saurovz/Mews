namespace Mews.Job.Scheduler.Domain.Jobs;

public sealed class JobFilters
{
    public JobFilters(
        IEnumerable<Guid>? ids = null,
        string? name = null,
        IEnumerable<string>? executorTypeNames = null,
        JobStates? states = null,
        DateTime? startUtc = null,
        DateTime? endUtc = null,
        bool showDeleted = false,
        Limitation? limitation = null)
    {
        Ids = ids;
        Name = name;
        ExecutorTypeNames = executorTypeNames;
        States = states;
        StartUtc = startUtc;
        EndUtc = endUtc;
        ShowDeleted = showDeleted;
        Limitation = limitation;
    }

    public IEnumerable<Guid>? Ids { get; }
    
    public string? Name { get; }

    public IEnumerable<string>? ExecutorTypeNames { get; }

    public JobStates? States { get; }

    public DateTime? StartUtc { get; }

    public DateTime? EndUtc { get; }

    public bool ShowDeleted { get; }

    public Limitation? Limitation { get; }
}
