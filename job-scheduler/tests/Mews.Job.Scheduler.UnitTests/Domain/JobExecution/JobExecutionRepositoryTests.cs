using System.Collections.Immutable;
using Mews.Atlas.FeatureFlags;
using Mews.Job.Scheduler.Configuration;
using Mews.Job.Scheduler.Core.EntityFrameworkCore;
using Mews.Job.Scheduler.Core.EntityFrameworkCore.JobExecutions;
using Mews.Job.Scheduler.Domain.JobExecutions;
using Mews.Job.Scheduler.Domain.Jobs;
using Mews.Job.Scheduler.Tests;
using Mews.Job.Scheduler.UnitTests.Data;
using Microsoft.EntityFrameworkCore;

namespace Mews.Job.Scheduler.UnitTests;

public sealed class JobExecutionRepositoryTests
{
    private InMemoryDbContextFactory _dbContextFactory = null!;

    [SetUp]
    public async Task SetUp()
    {
        _dbContextFactory = await InMemoryDbContextFactory.SetUpInMemoryDatabase(async context =>
        {
            var referencePendingJob = TestData.CreateJob(JobState.InProgress);
            var referenceExecutedJob = TestData.CreateJob(JobState.Executed, isDeleted: true);
            var referencePendingDeletedJob = TestData.CreateJob(JobState.InProgress, isDeleted: true);
            await context.AddRangeAsync(
                referencePendingJob,
                TestData.CreateJobExecution(JobExecutionState.InProgress, job: referencePendingJob, isDeleted: true),
                TestData.CreateJobExecution(JobExecutionState.InProgress, job: referencePendingJob),
                referenceExecutedJob,
                TestData.CreateJobExecution(JobExecutionState.Success, job: referenceExecutedJob, isDeleted: true),
                TestData.CreateJobExecution(JobExecutionState.Success, job: referenceExecutedJob),
                referencePendingDeletedJob,
                TestData.CreateJobExecution(JobExecutionState.InProgress, job: referencePendingDeletedJob, isDeleted: true),
                TestData.CreateJobExecution(JobExecutionState.InProgress, job: referencePendingDeletedJob)
            );
        });
    }

    [TearDown]
    public async Task TearDown()
    {
        await _dbContextFactory.DisposeAsync();
    }

    [Test]
    [TestCase(false)]
    [TestCase(true)]
    public async Task GetJobExecutionsToTimeout_ReturnsValidExecutions(bool isBatchProcessingAllowed)
    {
        var context = _dbContextFactory.CreateContext();
        var jobs = await context.Jobs.Where(j => j.State == JobState.InProgress && j.IsDeleted == false).ToListAsync();
        var featureFlagOptions = new FeatureFlagOptions(new List<FeatureFlag>());
        var sut = new JobExecutionRepository(context);

        var executions = await sut.GetJobExecutionsToTimeoutAsync(
            jobs: jobs,
            cancellationToken: CancellationToken.None
        );

        ClassicAssert.IsNotEmpty(executions);
        var jobIds = jobs.Select(j => j.Id).ToImmutableHashSet();
        ClassicAssert.IsTrue(executions.All(execution => (
            execution is { State: JobExecutionState.InProgress, IsDeleted: false } &&
            jobIds.Contains(execution.JobId)
        )));
    }
}
