using Mews.Job.Scheduler.BuildingBlocks.Types.DateTimeSpan;
using Mews.Job.Scheduler.Domain.Jobs;
using Mews.Job.Scheduler.Domain.Profiles;
using Mews.Job.Scheduler.Tests;

namespace Mews.Job.Scheduler.UnitTests;

[TestFixture]
public sealed class JobMarkAsTimedOutTests
{
    [Test]
    public void MarkAsTimedOut_NonPeriodicalJob_Succeeds()
    {
        var nowUtc = DateTime.UtcNow;
        var updaterProfile = new SystemProfile();
        var arrangedJob = TestData.CreateJob(
            state: JobState.InProgress,
            createdUtc: nowUtc.AddDays(-1)
        );

        arrangedJob.MarkAsTimedOut(nowUtc, 0, updaterProfile);

        AssertJobWasUpdatedToExecutedAndSoftDeleted(arrangedJob, nowUtc, updaterProfile);
    }

    [Test]
    [TestCase(0, Description = "First timeout.")]
    [TestCase(2, Description = "Repeated timeout.")]
    public void MarkAsTimedOut_PeriodicalJob_Succeeds(int retryCount)
    {
        var nowUtc = DateTime.UtcNow;
        var updaterProfile = new SystemProfile();
        var arrangedJob = TestData.CreateJob(
            state: JobState.InProgress,
            createdUtc: nowUtc.AddDays(-1),
            period: new DateTimeSpan(minutes: 5)
        );
        var startUtc = JobHelpers.GetNextStartUtc(
            startUtc: arrangedJob.StartUtc,
            period: arrangedJob.Period!.Value,
            nowUtc: nowUtc,
            jobTimeoutRetryDisabled: arrangedJob.TimeoutRetryDisabled,
            timeoutRetryCount: retryCount,
            lastExecutionTimedOut: true
        );

        arrangedJob.MarkAsTimedOut(nowUtc, timeoutRetryCount: retryCount, updaterProfile);

        AssertJobWasUpdatedToPending(arrangedJob, nowUtc, startUtc, updaterProfile);
    }

    [Test]
    public void MarkAsTimedOut_PeriodicalJobTimeoutRetryDisabled_Succeeds()
    {
        var nowUtc = DateTime.UtcNow;
        var createdUtc = nowUtc.AddDays(-1);
        var updaterProfile = new SystemProfile();
        var arrangedJob = TestData.CreateJob(
            state: JobState.InProgress,
            options: JobOptions.TimeoutRetryDisabled,
            createdUtc: createdUtc,
            period: new DateTimeSpan(minutes: 5)
        );
        var startUtc = JobHelpers.GetNextStartUtc(
            startUtc: arrangedJob.StartUtc,
            period: arrangedJob.Period!.Value,
            nowUtc: nowUtc,
            jobTimeoutRetryDisabled: arrangedJob.TimeoutRetryDisabled,
            timeoutRetryCount: 0,
            lastExecutionTimedOut: true
        );

        arrangedJob.MarkAsTimedOut(nowUtc, timeoutRetryCount: 0, updaterProfile);

        AssertJobWasUpdatedToPending(arrangedJob, nowUtc, startUtc, updaterProfile);
    }

    private static void AssertJobWasUpdatedToPending(
        Domain.Jobs.Job job,
        DateTime nowUtc,
        DateTime startUtc,
        SystemProfile updaterProfile)
    {
        ClassicAssert.AreEqual(nowUtc, job.UpdatedUtc);
        ClassicAssert.AreEqual(updaterProfile.Id, job.UpdaterProfileId);
        ClassicAssert.AreEqual(JobState.Pending, job.State);
        ClassicAssert.AreEqual(startUtc, job.StartUtc);
    }

    private static void AssertJobWasUpdatedToExecutedAndSoftDeleted(
        Domain.Jobs.Job job,
        DateTime nowUtc,
        SystemProfile updaterProfile)
    {
        ClassicAssert.AreEqual(nowUtc, job.UpdatedUtc);
        ClassicAssert.AreEqual(updaterProfile.Id, job.UpdaterProfileId);
        ClassicAssert.AreEqual(JobState.Executed, job.State);
        ClassicAssert.IsTrue(job.IsDeleted);
        ClassicAssert.IsNotNull(job.DeletedUtc);
        ClassicAssert.AreEqual(nowUtc, job.DeletedUtc);
    }
}
