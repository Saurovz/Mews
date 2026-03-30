using Mews.Atlas.Temporal.Schedules;
using Temporalio.Client.Schedules;

namespace Mews.Job.Scheduler.Workflows.JobExecutionCleaner;

public static class JobExecutionCleanerSchedule
{
    public static ScheduleDefinition CreateDefinition(TimeSpan interval)
    {
        return new ScheduleDefinition(
            "job-execution-cleaner-schedule",
            "JobExecutionCleanerWorkflow",
            "job-execution-cleaner-scheduled-workflow",
            [],
            new ScheduleSpec
            {
                Intervals = [new ScheduleIntervalSpec(interval)]
            });
    }
}
