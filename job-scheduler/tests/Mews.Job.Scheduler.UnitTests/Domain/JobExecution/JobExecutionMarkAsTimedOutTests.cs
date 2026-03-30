using Mews.Job.Scheduler.Domain.JobExecutions;
using Mews.Job.Scheduler.Domain.Profiles;
using Mews.Job.Scheduler.Tests;

namespace Mews.Job.Scheduler.UnitTests;

[TestFixture]
public sealed class JobExecutionMarkAsTimedOutTests
{
    [Test]
    public void MarkAsTimedOut_JobExecution_Succeeds()
    {
        var nowUtc = DateTime.UtcNow;
        var updaterProfile = new SystemProfile();
        var execution = TestData.CreateJobExecution(
            state: JobExecutionState.InProgress,
            createdUtc: nowUtc.AddDays(-1)
        );

        execution.MarkAsTimedOut(nowUtc, updaterProfile);

        AssertJobExecutionWasUpdatedToTimedOutWithEndTime(execution, nowUtc, updaterProfile);
    }

    private static void AssertJobExecutionWasUpdatedToTimedOutWithEndTime(
        JobExecution execution,
        DateTime nowUtc,
        SystemProfile updaterProfile)
    {
        ClassicAssert.AreEqual(nowUtc, execution.UpdatedUtc);
        ClassicAssert.AreEqual(updaterProfile.Id, execution.UpdaterProfileId);
        ClassicAssert.AreEqual(JobExecutionState.Timeout, execution.State);
        ClassicAssert.IsNotNull(execution.EndUtc);
        ClassicAssert.AreEqual(nowUtc, execution.EndUtc);
    }
}
