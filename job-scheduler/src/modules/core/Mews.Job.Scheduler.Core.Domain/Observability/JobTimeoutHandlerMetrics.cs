using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Mews.Job.Scheduler.Observability;

public sealed class JobTimeoutHandlerMetrics
{
    public const string MeterName = "Mews.Job.Scheduler.JobTimeoutHandler";

    private readonly Counter<int> _timedOutJobs;
    private readonly Counter<int> _timedOutJobExecutions;
    private readonly Histogram<double> _executionTime;

    public JobTimeoutHandlerMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);

        _timedOutJobs = meter.CreateCounter<int>("mews.job.scheduler.jobTimeoutHandler.jobsTimedOut", "job",
            "Total number of detected timed out jobs");
        _timedOutJobExecutions = meter.CreateCounter<int>("mews.job.scheduler.jobTimeoutHandler.executionsTimedOut", "execution",
            "Total number of detected timed out job executions");
        _executionTime = meter.CreateHistogram<double>("mews.job.scheduler.jobTimeoutHandler.executionTime", "milliseconds",
            "Execution time of the job timeout handler");
    }

    public TagList GetTimedOutJobTagList(Guid jobId, string jobFullName)
    {
        return new TagList
        {
            { "mews.jobId", jobId },
            { "mews.jobFullName", jobFullName }
        };
    }

    public TagList GetTimedOutExecutionTagList(Guid executionId, Guid jobId)
    {
        return new TagList
        {
            { "mews.executionId", executionId },
            { "mews.jobId", jobId }
        };
    }

    public void IncrementTimedOutJob(TagList tagList)
    {
        _timedOutJobs.Add(1, tagList);
    }

    public void IncrementTimedOutExecution(TagList tagList)
    {
        _timedOutJobExecutions.Add(1, tagList);
    }

    public void RecordExecutionTime(TimeSpan executionDuration)
    {
        _executionTime.Record(executionDuration.TotalMilliseconds);
    }
}
