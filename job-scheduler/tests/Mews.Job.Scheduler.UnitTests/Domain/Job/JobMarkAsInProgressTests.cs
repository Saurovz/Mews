using Mews.Job.Scheduler.Domain.Jobs;
using Mews.Job.Scheduler.Domain.Profiles;
using Mews.Job.Scheduler.Job;
using Mews.Job.Scheduler.Tests;

namespace Mews.Job.Scheduler.UnitTests;

[TestFixture]
public sealed class JobMarkAsInProgressTests
{
    [Test]
    public void TryMarkAsInProgress_SetsJobStateToInProgress_WhenJobStateIsScheduled()
    {
        var nowUtc = DateTime.UtcNow;
        var updaterProfile = new SystemProfile();
        var sut = TestData.CreateJob(JobState.Scheduled);

        sut.TryMarkAsInProgress(nowUtc, updaterProfile);

        Assert.That(sut.State, Is.EqualTo(JobState.InProgress));
        Assert.That(sut.ExecutionStartUtc, Is.EqualTo(nowUtc));
        Assert.That(sut.UpdatedUtc, Is.EqualTo(nowUtc));
        Assert.That(sut.UpdaterProfileId, Is.EqualTo(updaterProfile.Id));
    }

    [Test]
    public void TryMarkAsInProgress_ReturnsUnchanged_WhenJobStateIsInProgress()
    {
        var nowUtc = DateTime.UtcNow;
        var updaterProfile = new SystemProfile();
        var sut = TestData.CreateJob(JobState.Scheduled);
        sut.TryMarkAsInProgress(nowUtc, updaterProfile);

        sut.TryMarkAsInProgress(DateTime.UtcNow, updaterProfile);

        Assert.That(sut.State, Is.EqualTo(JobState.InProgress));
        Assert.That(sut.ExecutionStartUtc, Is.EqualTo(nowUtc));
        Assert.That(sut.UpdatedUtc, Is.EqualTo(nowUtc));
        Assert.That(sut.UpdaterProfileId, Is.EqualTo(updaterProfile.Id));
    }

    [Test]
    public void TryMarkAsInProgress_ThrowsException_WhenJobStateIsNotScheduled()
    {
        var nowUtc = DateTime.UtcNow;
        var updaterProfile = new SystemProfile();
        var sut = TestData.CreateJob(JobState.Executed);

        Assert.Throws<EntityProcessingException>(() => sut.TryMarkAsInProgress(nowUtc, updaterProfile));
    }
}
