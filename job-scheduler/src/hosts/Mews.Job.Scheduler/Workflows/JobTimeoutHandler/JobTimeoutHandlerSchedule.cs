using Mews.Atlas.Temporal.Schedules;
using Temporalio.Client.Schedules;

namespace Mews.Job.Scheduler.Workflows.JobTimeoutHandler;

public static class JobTimeoutHandlerSchedule
{
    public static ScheduleDefinition CreateDefinition(TimeSpan interval)
    {
        return new ScheduleDefinition(
            "job-timeout-handler-schedule",
            "JobTimeoutHandlerWorkflow",
            "job-timeout-handler-scheduled-workflow",
            [],
            new ScheduleSpec
            {
                Intervals = [new ScheduleIntervalSpec(interval)]
            });
    }
}
