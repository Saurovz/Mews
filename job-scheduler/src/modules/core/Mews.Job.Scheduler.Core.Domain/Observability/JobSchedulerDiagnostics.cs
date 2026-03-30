using System.Diagnostics;
using System.Reflection;
using Mews.Job.Scheduler.Observability.Events;

namespace Mews.Job.Scheduler.Observability;

public static class JobSchedulerDiagnostics
{
    public const string ActivitySourceName = "Mews.Job.Scheduler.JobSchedulerService";
    public static readonly ActivitySource Source = new(ActivitySourceName, Assembly.GetExecutingAssembly().GetName().Version?.ToString());

    private const string JobToScheduleMessageCreatedEventName = "jobToScheduleMessage.created";
    private const string AttributeJobToScheduleMessageCreatedEventJobId = "jobId";
    private const string AttributeJobToScheduleMessageCreatedEventFullName= "fullName";

    public static void AddJobToScheduleMessageCreatedEvent(this Activity? source, JobToScheduleMessageCreatedEvent createdEvent)
    {
        source?.AddEvent(new ActivityEvent(
            JobToScheduleMessageCreatedEventName,
            tags: new ActivityTagsCollection(new List<KeyValuePair<string, object?>>
            {
                new(AttributeJobToScheduleMessageCreatedEventJobId, createdEvent.JobId),
                new(AttributeJobToScheduleMessageCreatedEventFullName, createdEvent.JobFullName)
            })
        ));
    }
}
