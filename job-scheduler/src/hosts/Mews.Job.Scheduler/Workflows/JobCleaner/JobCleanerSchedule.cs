using Mews.Atlas.Temporal.Schedules;
using Temporalio.Client.Schedules;

namespace Mews.Job.Scheduler.Workflows.JobCleaner;

public static class JobCleanerSchedule
{
    public static ScheduleDefinition CreateDefinition(TimeSpan interval)
    {
        return new ScheduleDefinition(
            "job-cleaner-schedule",
            "JobCleanerWorkflow",
            "job-cleaner-scheduled-workflow",
            [],
            new ScheduleSpec
            {
                Intervals = [new ScheduleIntervalSpec(interval)]
            });
    }
}
