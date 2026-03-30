namespace Mews.Job.Scheduler.Domain.Jobs;

public interface IHaveStartTime
{
    DateTime StartUtc { get; }
}
