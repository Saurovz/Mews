using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Mews.Job.Scheduler.Observability;

public sealed class JobSchedulerMetrics
{
    public const string MeterName = "Mews.Job.Scheduler.JobScheduler";

    private readonly Histogram<double> _jobSchedulerExecutionDelay;
    private readonly Histogram<double> _jobSchedulerTimeSinceLastSuccessfulExecution;

    public JobSchedulerMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);

        _jobSchedulerExecutionDelay = meter.CreateHistogram<double>("mews.job.scheduler.jobScheduler.executionDelay", "ms", "Job execution delay");
        _jobSchedulerTimeSinceLastSuccessfulExecution = meter.CreateHistogram<double>("mews.job.scheduler.jobScheduler.timeSinceSuccess", "ms", "Time since last successful execution");
    }

    public TagList GetDefaultTags(Guid jobId, string fullName)
    {
        var tags = new TagList
        {
            { "mews.jobId", jobId },
            { "mews.jobFullName", fullName }
        };
        return tags;
    }

    public void RecordExecutionDelay(double count, TagList tagList)
    {
        _jobSchedulerExecutionDelay.Record(count, tagList);
    }

    public void RecordTimeSinceLastSuccess(double milliseconds, TagList tagList)
    {
        _jobSchedulerTimeSinceLastSuccessfulExecution.Record(milliseconds, tagList);
    }
}
