using System.Diagnostics;

namespace Mews.Job.Scheduler.Observability;

public static class JobProcessingDiagnostics
{
    public const string ActivitySourceName = "Mews.Job.Scheduler.Processing";
    public static readonly ActivitySource Source = new(ActivitySourceName);
    
    public static void AddJobTimeOutCheckEvent(Activity? activity, Domain.Jobs.Job job, DateTime nowUtc)
    {
        activity?.AddEvent(new ActivityEvent(
            name: "job.timeOut.check",
            tags: new ActivityTagsCollection
            {
                { "job.id", job.Id },
                { "job.fullname", job.FullName },
                { "job.timedOut", job.IsTimedOut(nowUtc)},
                { "job.executionStartUtc", job.ExecutionStartUtc}
            }));
    }
}
