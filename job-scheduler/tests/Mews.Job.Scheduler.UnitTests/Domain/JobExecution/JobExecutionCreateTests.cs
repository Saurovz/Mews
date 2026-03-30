using Mews.Job.Scheduler.Domain.JobExecutions;
using Mews.Job.Scheduler.Domain.Profiles;

namespace Mews.Job.Scheduler.UnitTests;

[TestFixture]
public sealed class JobExecutionCreateTests
{
    [Test]
    public void Create_JobExecution_Succeeds()
    {
        var nowUtc = DateTime.UtcNow;
        var creatorProfile = new SystemProfile();
        var parameters = new JobExecutionCreateParameters(
            JobId: Guid.NewGuid(),
            ExecutorTypeNameValue: "Executor",
            State: JobExecutionState.InProgress,
            StartUtc: nowUtc.AddDays(-1),
            TransactionIdentifier: "Transaction"
        );

        var sut = JobExecution.Create(parameters, nowUtc, creatorProfile);

        Assert.That(sut.JobId, Is.EqualTo(parameters.JobId));
        Assert.That(sut.ExecutorTypeNameValue, Is.EqualTo(parameters.ExecutorTypeNameValue));
        Assert.That(sut.State, Is.EqualTo(parameters.State));
        Assert.That(sut.StartUtc, Is.EqualTo(parameters.StartUtc));
        Assert.That(sut.TransactionIdentifier, Is.EqualTo(parameters.TransactionIdentifier));
        Assert.That(sut.CreatedUtc, Is.EqualTo(nowUtc));
        Assert.That(sut.CreatorProfileId, Is.EqualTo(creatorProfile.Id));
        Assert.That(sut.UpdatedUtc, Is.EqualTo(nowUtc));
        Assert.That(sut.UpdaterProfileId, Is.EqualTo(creatorProfile.Id));
    }
}
