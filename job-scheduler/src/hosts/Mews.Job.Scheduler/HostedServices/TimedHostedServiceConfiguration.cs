namespace Mews.Job.Scheduler.HostedServices;

public sealed class TimedHostedServiceConfiguration
{
    public TimeSpan Period { get; set; }

    public WorkerStoppingBehavior StoppingBehavior { get; set; } = WorkerStoppingBehavior.Immediate;

    public bool IsEnabled { get; set; } = true;

    public Dictionary<string, object>? AdditionalParameters { get; set; }
}
