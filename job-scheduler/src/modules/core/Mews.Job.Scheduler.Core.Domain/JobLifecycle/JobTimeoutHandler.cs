using System.Diagnostics;
using Mews.Atlas.Alerting;
using Mews.Atlas.FeatureFlags;
using Mews.Job.Scheduler.Configuration;
using Mews.Job.Scheduler.Domain.DateTimeProviders;
using Mews.Job.Scheduler.Domain.JobExecutions;
using Mews.Job.Scheduler.Domain.Profiles;
using Mews.Job.Scheduler.Observability;
using Microsoft.Extensions.DependencyInjection;

namespace Mews.Job.Scheduler.Domain.JobLifecycle;

public sealed class JobTimeoutHandler
{
    private readonly IIncidentReporter _incidentReporter;
    private readonly IServiceProvider _serviceProvider;
    private readonly IJobPersistence _jobPersistence;
    private readonly SystemProfile _systemProfile;
    private readonly IDateTimeProvider _dateTime;
    private readonly IFeatureFlagService _featureFlags;
    private readonly JobTimeoutHandlerMetrics _metrics;
    private readonly JobTimeoutRetryCache _timeoutRetryCache;

    public JobTimeoutHandler(
        IIncidentReporter incidentReporter,
        IServiceProvider serviceProvider,
        IJobPersistence jobPersistence,
        SystemProfile systemProfile,
        IDateTimeProvider dateTime,
        IFeatureFlagService featureFlags,
        JobTimeoutHandlerMetrics metrics,
        JobTimeoutRetryCache timeoutRetryCache)
    {
        _incidentReporter = incidentReporter;
        _serviceProvider = serviceProvider;
        _jobPersistence = jobPersistence;
        _systemProfile = systemProfile;
        _dateTime = dateTime;
        _featureFlags = featureFlags;
        _metrics = metrics;
        _timeoutRetryCache = timeoutRetryCache;
    }

    public async Task FindAndMarkAsTimedOut(CancellationToken applicationStoppingToken)
    {
        var duration = Stopwatch.StartNew();
        var nowUtc = _dateTime.NowUtc;
        var jobs = await _jobPersistence.Jobs.GetJobsToTimeoutAsync(nowUtc, applicationStoppingToken);
        LogDiscoveredJobs(jobs);
        MarkAsTimedOut(jobs, nowUtc);

        var executions = await _jobPersistence.JobExecutions.GetJobExecutionsToTimeoutAsync(jobs, applicationStoppingToken);
        LogDiscoveredJobExecutions(executions);
        MarkAsTimedOutAndReport(executions, nowUtc);

        await _jobPersistence.SaveChangesAsync(applicationStoppingToken);
        _metrics.RecordExecutionTime(duration.Elapsed);
    }

    private void LogDiscoveredJobs(IReadOnlyCollection<Domain.Jobs.Job> jobsToTimeout)
    {
        foreach (var job in jobsToTimeout)
        {
            _metrics.IncrementTimedOutJob(_metrics.GetTimedOutJobTagList(job.Id, job.FullName));
        }
    }

    private void MarkAsTimedOut(List<Domain.Jobs.Job> jobsToTimeout, DateTime nowUtc)
    {
        foreach (var job in jobsToTimeout)
        {
            var retryAttempt = _timeoutRetryCache.Increment(job.Id);
            job.MarkAsTimedOut(nowUtc, retryAttempt, _systemProfile);
        }
    }

    private void LogDiscoveredJobExecutions(IReadOnlyCollection<JobExecution> jobExecutionsToTimeout)
    {
        foreach (var execution in jobExecutionsToTimeout)
        {
            _metrics.IncrementTimedOutExecution(_metrics.GetTimedOutExecutionTagList(execution.Id, execution.JobId));
        }
    }

    private void MarkAsTimedOutAndReport(List<JobExecution> jobExecutionsToTimeout, DateTime nowUtc)
    {
        foreach (var execution in jobExecutionsToTimeout)
        {
            execution.MarkAsTimedOut(nowUtc, _systemProfile);
        }

        ReportTimeout(jobExecutionsToTimeout);
    }

    private void ReportTimeout(List<JobExecution> timedOutExecutions)
    {
        foreach (var execution in timedOutExecutions)
        {
            var exception = JobExecutionTimeoutException.Create(execution);
            var reporter = GetReporter(exception.Team);
            reporter.Report(
                title: exception.Message,
                team: exception.Team,
                level: exception.IncidentLevel,
                exception: exception,
                details: exception.Details
            );
        }
    }

    private IIncidentReporter GetReporter(string team)
    {
        return _serviceProvider.GetKeyedService<IIncidentReporter>(team) ?? _incidentReporter;
    }
}
