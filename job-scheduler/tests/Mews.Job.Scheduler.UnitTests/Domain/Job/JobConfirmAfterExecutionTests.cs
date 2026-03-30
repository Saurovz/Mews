using Mews.Job.Scheduler.BuildingBlocks.Types.DateTimeSpan;
using Mews.Job.Scheduler.Domain.Jobs;
using Mews.Job.Scheduler.Domain.Profiles;
using Mews.Job.Scheduler.Job;
using Mews.Job.Scheduler.Tests;

namespace Mews.Job.Scheduler.UnitTests;

[TestFixture]
public sealed class JobConfirmAfterExecutionTests
{
    [Test]
    public void ConfirmAfterExecution_WhenStateIsInProgressAndJobIsPeriodical_Updates()
    {
        var nowUtc = DateTime.UtcNow;
        var systemProfile = new SystemProfile();
        var confirmParameters = new ConfirmJobAfterExecutionParameters(
            IsExecutionSuccess: true,
            IsExecutionTimedOut: false,
            TimeoutRetryCount: 0,
            DeleteJob: false,
            FutureRunData: "The quick brown fox jumps over the lazy dog"
        );
        var sut = TestData.CreateJob(JobState.InProgress, period: new DateTimeSpan(minutes: 10));
        var startUtc = sut.StartUtc;
        var nextStartUtc = JobHelpers.GetNextStartUtc(
            startUtc: startUtc,
            period: sut.Period!.Value,
            nowUtc: nowUtc,
            jobTimeoutRetryDisabled: false,
            timeoutRetryCount: 0,
            lastExecutionTimedOut: false
        );

        sut.ConfirmAfterExecution(confirmParameters, nowUtc, systemProfile);

        Assert.That(sut.PreviousSuccessfulStartUtc, Is.EqualTo(startUtc));
        Assert.That(sut.Data, Is.EqualTo(confirmParameters.FutureRunData));
        Assert.That(sut.StartUtc, Is.EqualTo(nextStartUtc));
        Assert.That(sut.ExecutionStartUtc, Is.Null);
        Assert.That(sut.State, Is.EqualTo(JobState.Pending));
        Assert.That(sut.UpdaterProfileId, Is.EqualTo(systemProfile.Id));
        Assert.That(sut.UpdatedUtc, Is.EqualTo(nowUtc));
    }

    [Test]
    public void ConfirmAfterExecution_WhenDeleteJobIsTrue_UpdatesToExecuted()
    {
        var confirmParameters = new ConfirmJobAfterExecutionParameters(
            IsExecutionSuccess: true,
            IsExecutionTimedOut: false,
            TimeoutRetryCount: 0,
            DeleteJob: true
        );
        var sut = TestData.CreateJob(JobState.InProgress, period: new DateTimeSpan(minutes: 10));

        sut.ConfirmAfterExecution(confirmParameters,  DateTime.UtcNow, new SystemProfile());

        Assert.That(sut.State, Is.EqualTo(JobState.Executed));
    }

    [Test]
    public void ConfirmAfterExecution_WhenStateIsInProgressAndJobIsNotPeriodical_SetsStateToExecuted()
    {
        var nowUtc = DateTime.UtcNow;
        var systemProfile = new SystemProfile();
        var confirmParameters = new ConfirmJobAfterExecutionParameters(
            IsExecutionSuccess: true,
            IsExecutionTimedOut: false,
            TimeoutRetryCount: 0,
            DeleteJob: false
        );
        var sut = TestData.CreateJob(JobState.InProgress);

        sut.ConfirmAfterExecution(confirmParameters, nowUtc, systemProfile);

        Assert.That(sut.State, Is.EqualTo(JobState.Executed));
        Assert.That(sut.UpdaterProfileId, Is.EqualTo(systemProfile.Id));
        Assert.That(sut.UpdatedUtc, Is.EqualTo(nowUtc));
    }

    [Test]
    public void ConfirmAfterExecution_WhenStateIsNotInProgress_ThrowsInvalidOperationException()
    {
        var confirmParameters = new ConfirmJobAfterExecutionParameters(
            IsExecutionSuccess: true,
            IsExecutionTimedOut: false,
            TimeoutRetryCount: 0,
            DeleteJob: true
        );
        var sut = TestData.CreateJob(JobState.Executed);

        Assert.Throws<StateTransitionException>(() => sut.ConfirmAfterExecution(confirmParameters, DateTime.UtcNow, new SystemProfile()));
    }
}
