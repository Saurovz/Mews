using Mews.Job.Scheduler.Domain.JobExecutions;
using Mews.Job.Scheduler.Domain.Profiles;
using Mews.Job.Scheduler.Job;
using Mews.Job.Scheduler.Tests;

namespace Mews.Job.Scheduler.UnitTests;

[TestFixture]
public sealed class JobExecutionUpdateJobExecutionResultTests
{
    [Test]
    public void UpdateJobExecutionResult_WhenStateIsInProgress_Succeeds()
    {
        var nowUtc = DateTime.UtcNow;
        var systemProfile = new SystemProfile();
        var resultParameters = new UpdateJobExecutionResultParameters(JobExecutionState.Success, "Tag", nowUtc);
        var sut = TestData.CreateJobExecution(JobExecutionState.InProgress);

        sut.UpdateJobExecutionResult(resultParameters, nowUtc, systemProfile);

        Assert.That(sut.State, Is.EqualTo(resultParameters.State));
        Assert.That(sut.EndUtc, Is.EqualTo(resultParameters.EndUtc));
        Assert.That(sut.Tag, Is.EqualTo(resultParameters.Tag));
        Assert.That(sut.UpdaterProfileId, Is.EqualTo(systemProfile.Id));
        Assert.That(sut.UpdatedUtc, Is.EqualTo(nowUtc));
    }

    [Test]
    public void UpdateJobExecutionResult_WhenStateIsNotInProgress_ThrowsInvalidOperationException()
    {
        var resultParameters = new UpdateJobExecutionResultParameters(JobExecutionState.Error, "Tag", DateTime.UtcNow);
        var sut = TestData.CreateJobExecution(JobExecutionState.Success);

        Assert.Throws<StateTransitionException>(() => sut.UpdateJobExecutionResult(resultParameters, DateTime.UtcNow, new SystemProfile()));
    }
}
