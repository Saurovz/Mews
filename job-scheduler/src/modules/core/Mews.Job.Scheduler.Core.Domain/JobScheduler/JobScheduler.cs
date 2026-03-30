using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using Mews.Atlas.Alerting;
using Mews.Atlas.Messaging.Exceptions;
using Mews.Atlas.OpenTelemetry;
using Mews.Job.Scheduler.BuildingBlocks.Types.Extensions;
using Mews.Job.Scheduler.BuildingBlocks.Domain.PlatformTeams;
using Mews.Job.Scheduler.Core.Messaging.Jobs;
using Mews.Job.Scheduler.Domain.DateTimeProviders;
using Mews.Job.Scheduler.Domain.Profiles;
using Mews.Job.Scheduler.Observability;
using Mews.Job.Scheduler.Observability.Events;

namespace Mews.Job.Scheduler.Domain.JobScheduler;

public sealed class JobScheduler
{
    private readonly SystemProfile _systemProfile;
    private readonly IJobPersistence _jobPersistence;
    private readonly IJobPublisher _jobPublisher;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IIncidentReporter _incidentReporter;
    private readonly JobSchedulerMetrics _metrics;

    public JobScheduler(
        SystemProfile systemProfile,
        IJobPersistence jobPersistence,
        IJobPublisher jobPublisher,
        IDateTimeProvider dateTimeProvider,
        IIncidentReporter incidentReporter,
        JobSchedulerMetrics metrics)
    {
        _systemProfile = systemProfile;
        _jobPersistence = jobPersistence;
        _jobPublisher = jobPublisher;
        _dateTimeProvider = dateTimeProvider;
        _incidentReporter = incidentReporter;
        _metrics = metrics;
    }

    public async Task ScheduleNextBatchAsync(CancellationToken cancellationToken)
    {
        using var activity = JobSchedulerDiagnostics.Source.StartActivity(parentContext: Activity.Current?.Context);
        
        try
        {
            var nowUtc = _dateTimeProvider.NowUtc;
            var jobs = await _jobPersistence.Jobs.GetNextBatchToScheduleAsync(nowUtc, cancellationToken);

            if (jobs.Count > 0)
            {
                var messagesToPublish = new List<JobQueueMessage>();
                foreach (var job in jobs)
                {
                    activity?.AddJobToScheduleMessageCreatedEvent(new JobToScheduleMessageCreatedEvent(job.Id, job.FullName));

                    var executionDelayInMs = (nowUtc - job.StartUtc).TotalMilliseconds;
                    _metrics.RecordExecutionDelay(executionDelayInMs, _metrics.GetDefaultTags(job.Id, job.FullName));

                    var timeSinceLastSuccessfulExecutionInMs = (nowUtc - (job.PreviousSuccessfulStartUtc ?? job.CreatedUtc)).TotalMilliseconds;
                    _metrics.RecordTimeSinceLastSuccess(timeSinceLastSuccessfulExecutionInMs, _metrics.GetDefaultTags(job.Id, job.FullName));

                    job.TryMarkAsScheduled(nowUtc, _systemProfile);
                    messagesToPublish.Add(GetJobQueueMessage(job));
                }

                await _jobPersistence.SaveChangesAsync(cancellationToken);
                await _jobPublisher.PublishAsync(messagesToPublish, cancellationToken);
            }
        }
        catch (MessageBatchSendingException exception) when (exception.InnerException is ServiceBusException { IsTransient: true })
        {
            activity?.RecordExceptionWithStatus(exception.InnerException);
            _incidentReporter.Report(exception.InnerException.Message, PlatformTeams.Tooling, IncidentLevel.Warning, exception.InnerException);
        }
        catch (ConcurrencyException)
        {
            // Do nothing, another instance of the job scheduler service already scheduled the job.
        }
    }

    private static JobQueueMessage GetJobQueueMessage(Domain.Jobs.Job job)
    {
        var identifier = new JobQueueMessageIdentifier(job.Id, job.StartUtc).Value;

        return new JobQueueMessage(
            Identifier: identifier,
            Name: job.NameNew,
            JobId: job.Id,
            ExecutorType: job.Executor.Type,
            Team: job.Executor.Team,
            CreatedUtc: new DateTimeOffset(job.CreatedUtc, TimeSpan.Zero),
            CreatorProfileId: job.CreatorProfileId,
            UpdatedUtc: new DateTimeOffset(job.UpdatedUtc, TimeSpan.Zero),
            UpdaterProfileId: job.UpdaterProfileId,
            ScheduledExecutionStart: new DateTimeOffset(job.StartUtc, TimeSpan.Zero),
            ExecutionStart: job.ExecutionStartUtc.HasValue ? new DateTimeOffset(job.ExecutionStartUtc.Value, TimeSpan.Zero) : null,
            PreviousSuccessfulStart: job.PreviousSuccessfulStartUtc.HasValue ? new DateTimeOffset(job.PreviousSuccessfulStartUtc.Value, TimeSpan.Zero) : null,
            JobMaxExecutionTime: job.MaxExecutionTime.ToTimeSpan(),
            LogVerbosity: "Everything",
            State: job.State.ToString(),
            Options: job.Options.ToString().Split(", "),
            Data: job.Data,
            Period: job.PeriodValue
        );
    }
}
