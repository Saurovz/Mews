using FluentAssertions;
using Mews.Atlas.FeatureFlags;
using Mews.Job.Scheduler.Aspire.IntegrationTests.Fakes;
using Mews.Job.Scheduler.Aspire.IntegrationTests.Helpers;
using Mews.Job.Scheduler.Core.EntityFrameworkCore.JobExecutions;
using Mews.Job.Scheduler.Core.EntityFrameworkCore.JobPersistence;
using Mews.Job.Scheduler.Core.EntityFrameworkCore.Jobs;
using Mews.Job.Scheduler.Domain;
using Mews.Job.Scheduler.Domain.DateTimeProviders;
using Mews.Job.Scheduler.Domain.JobExecutionLifecycle;
using Mews.Job.Scheduler.Domain.Jobs;
using Mews.Job.Scheduler.Observability;
using Mews.Job.Scheduler.Tests;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Mews.Job.Scheduler.Aspire.IntegrationTests.Features.Jobs;

internal class JobCleanerTests : TestBase
{
    private IJobPersistence _jobPersistence;
    private ILogger<JobCleaner> _loggerMock;
    private IDateTimeProvider _dateTimeProviderMock;
    private JobCleanerMetrics _metricsMock;

    private JobCleaner _jobCleaner;

    [SetUp]
    public void SetUp()
    {
        // Explicitly clear database and data from other tests, the tests were deleting jobs from other tests in the suite.
        DatabaseHelpers.EnsureDatabaseIsCreated();

        var context = IntegrationTests.DbContextFactory.CreateDbContext();
        var jobRepository = new JobRepository(context);

        var executorRepository = new JobExecutionRepository(context);
        _jobPersistence = new JobPersistence(context, jobRepository, executorRepository);
        _loggerMock = Substitute.For<ILogger<JobCleaner>>();
        _dateTimeProviderMock = Substitute.For<IDateTimeProvider>();
        _metricsMock = new JobCleanerMetrics(new FakeMeterFactory());
        _jobCleaner = new JobCleaner(_jobPersistence, _loggerMock, _dateTimeProviderMock, _metricsMock);
    }

    [TestCase(-60, 30, 0, TestName = "WhenJobsOlderThanRetentionDays")]
    [TestCase(-30, 30, 0, TestName = "WhenJobsSameDayAsRetentionDays")]
    [TestCase(-10, 30, 3, TestName = "WhenJobsNewerThanRetentionDays")]
    public async Task Clean_ReturnsExpectedResult(int executionStartDeduction, int retentionDays, int expectedResultCount)
    {
        var nowUtc = DateTime.UtcNow;
        _dateTimeProviderMock.NowUtc.Returns(nowUtc);

        await TestAsync(
            async () =>
            {
                var jobs = await SampleData.AddJobRangeAsync(
                    IntegrationTests.DbContextFactory,
                    _ => 
                    [
                        TestData.CreateJob(JobState.Pending, startUtc: nowUtc.AddDays(executionStartDeduction), isDeleted: true),
                        TestData.CreateJob(JobState.InProgress, startUtc: nowUtc.AddDays(executionStartDeduction), isDeleted: true),
                        TestData.CreateJob(JobState.Executed, startUtc: nowUtc.AddDays(executionStartDeduction), isDeleted: true),
                    ],
                    default
                );

                return jobs.Select(j => j.Id).ToList();
            },
            async (_, token) =>
            {
                await _jobCleaner.CleanAsync(retentionDays, token);

                return Task.CompletedTask;
            },
            async (jobIds, _, ct) =>
            {
                var jobs = await _jobPersistence.Jobs.GetByIdsAsync(jobIds, includeExecutor: false, ct);
                jobs.Count.Should().Be(expectedResultCount);
            }
        );
    }
}

