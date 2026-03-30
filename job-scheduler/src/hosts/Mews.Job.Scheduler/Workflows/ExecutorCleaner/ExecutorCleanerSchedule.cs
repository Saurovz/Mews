using Mews.Atlas.Temporal.Schedules;
using Temporalio.Client.Schedules;

namespace Mews.Job.Scheduler.Workflows.ExecutorCleaner;

public static class ExecutorCleanerSchedule
{
    public static ScheduleDefinition CreateDefinition(TimeSpan interval)
    {
        return new ScheduleDefinition(
            "executor-cleaner-schedule",
            "ExecutorCleanerWorkflow",
            "executor-cleaner-scheduled-workflow",
            [],
            new ScheduleSpec
            {
                Intervals = [new ScheduleIntervalSpec(interval)]
            });
    }
}
