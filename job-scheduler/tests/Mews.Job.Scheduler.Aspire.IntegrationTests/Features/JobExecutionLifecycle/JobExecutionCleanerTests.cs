using Mews.Atlas.FeatureFlags;
using Mews.Job.Scheduler.Aspire.IntegrationTests.Fakes;
using Mews.Job.Scheduler.Aspire.IntegrationTests.Helpers;
using Mews.Job.Scheduler.Core.EntityFrameworkCore.JobExecutions;
using Mews.Job.Scheduler.Core.EntityFrameworkCore.JobPersistence;
using Mews.Job.Scheduler.Core.EntityFrameworkCore.Jobs;
using Mews.Job.Scheduler.Domain;
using Mews.Job.Scheduler.Domain.DateTimeProviders;
using Mews.Job.Scheduler.Domain.JobExecutionLifecycle;
using Mews.Job.Scheduler.Domain.JobExecutions;
using Mews.Job.Scheduler.Domain.Jobs;
using Mews.Job.Scheduler.Observability;
using Mews.Job.Scheduler.Tests;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Mews.Job.Scheduler.Aspire.IntegrationTests.Features.JobExecutionLifecycle;

[TestFixture]
internal sealed class JobExecutionCleanerTests: TestBase
{
    private IJobPersistence _jobPersistence;
    private ILogger<JobExecutionCleaner> _loggerMock;
    private IDateTimeProvider _dateTimeProviderMock;
    private JobExecutionCleanerMetrics _metricsMock;

    private JobExecutionCleaner _jobExecutionCleaner;

    [SetUp]
    public void SetUp()
    {
        // Explicitly clear database and data from other tests, the tests were deleting jobs from other tests in the suite.
        DatabaseHelpers.EnsureDatabaseIsCreated();

        var context = IntegrationTests.DbContextFactory.CreateDbContext();
        var jobRepository = new JobRepository(context);

        var executorRepository = new JobExecutionRepository(context);
        _jobPersistence = new JobPersistence(context, jobRepository, executorRepository);
        _loggerMock = Substitute.For<ILogger<JobExecutionCleaner>>();
        _dateTimeProviderMock = Substitute.For<IDateTimeProvider>();
        _metricsMock = new JobExecutionCleanerMetrics(new FakeMeterFactory());
        _jobExecutionCleaner = new JobExecutionCleaner(_jobPersistence, _loggerMock, _dateTimeProviderMock, _metricsMock);
    }

    [TestCase(-60, 30, 0, TestName = "WhenExecutionsOlderThanRetentionDays")]
    [TestCase(-30, 30, 0, TestName = "WhenExecutionsSameDayAsRetentionDays")]
    [TestCase(-10, 30, 30, TestName = "WhenExecutionsNewerThanRetentionDays")]
    public async Task Clean_ReturnsExpectedResult(int executionStartDeduction, int retentionDays,
        int expectedResultCount)
    {
        var nowUtc = DateTime.UtcNow;
        _dateTimeProviderMock.NowUtc.Returns(nowUtc);

        await TestAsync(
            arrange: async () =>
            {
                var job = await SampleData.AddJobAsync(IntegrationTests.DbContextFactory, _ => TestData.CreateJob(JobState.Pending, createdUtc: DateTime.UtcNow.AddDays(executionStartDeduction).AddSeconds(-1)), default);
                var jobExecutions = await SampleData.AddJobExecutionRangeAsync(IntegrationTests.DbContextFactory, _ => Enumerable.Range(1, 30).Select(_ => TestData.CreateJobExecution(JobExecutionState.Success, job)).ToList(), default);

                return jobExecutions.Select(je => je.Id).ToList();
            },
            act: async (_, token) =>
            {
                await _jobExecutionCleaner.CleanAsync(retentionDays, token);
                return Task.CompletedTask;
            },
            assert: async (executionIds, _, ct) =>
            {
                var jobExecutions = await _jobPersistence.JobExecutions.GetByIdsAsync(executionIds, ct);
                Assert.That(jobExecutions.Count, Is.EqualTo(expectedResultCount));
            }
        );
    }
}
