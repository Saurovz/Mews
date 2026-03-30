using Mews.Job.Scheduler.Core.EntityFrameworkCore.Jobs;
using Mews.Job.Scheduler.Domain.Jobs;
using Mews.Job.Scheduler.Tests;
using Mews.Job.Scheduler.UnitTests.Data;

namespace Mews.Job.Scheduler.UnitTests;

public sealed class JobRepositoryTests
{
    private InMemoryDbContextFactory _dbContextFactory = null!;

    [SetUp]
    public async Task SetUp()
    {
        _dbContextFactory = await InMemoryDbContextFactory.SetUpInMemoryDatabase(async context =>
        {
            await context.AddRangeAsync(
                TestData.CreateJob(JobState.Pending, isDeleted: true),
                TestData.CreateJob(JobState.Pending),
                TestData.CreateJob(JobState.InProgress, isDeleted: true, createdUtc: DateTime.UtcNow),
                TestData.CreateJob(JobState.InProgress, createdUtc: DateTime.UtcNow),
                TestData.CreateJob(JobState.InProgress, isDeleted: true),
                TestData.CreateJob(JobState.InProgress),
                TestData.CreateJob(JobState.Executed, isDeleted: true),
                TestData.CreateJob(JobState.Executed)
            );
        });
    }

    [TearDown]
    public async Task TearDown()
    {
        await _dbContextFactory.DisposeAsync();
    }

    [Test]
    public async Task GetJobsToTimeout_ReturnsValidJobs()
    {
        var context = _dbContextFactory.CreateContext();
        var sut = new JobRepository(context);

        var jobs = await sut.GetJobsToTimeoutAsync(DateTime.UtcNow, CancellationToken.None);

        ClassicAssert.IsNotEmpty(jobs);
        ClassicAssert.IsTrue(jobs.All(j => j is { State: JobState.InProgress, IsDeleted: false }));
    }
}
