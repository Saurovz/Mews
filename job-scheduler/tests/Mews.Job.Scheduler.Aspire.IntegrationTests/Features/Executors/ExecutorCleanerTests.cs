using FluentAssertions;
using Mews.Job.Scheduler.Aspire.IntegrationTests.Fakes;
using Mews.Job.Scheduler.Aspire.IntegrationTests.Helpers;
using Mews.Job.Scheduler.Core.EntityFrameworkCore.Executors;
using Mews.Job.Scheduler.Domain.DateTimeProviders;
using Mews.Job.Scheduler.Domain.Executors;
using Mews.Job.Scheduler.Domain.JobExecutionLifecycle;
using Mews.Job.Scheduler.Observability;
using Mews.Job.Scheduler.Tests;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Mews.Job.Scheduler.Aspire.IntegrationTests.Features.Executors;

[TestFixture]
internal sealed class ExecutorCleanerTests : TestBase
{
    [SetUp]
    public void SetUp()
    {
        // Explicitly clear database and data from other tests, the tests were deleting jobs from other tests in the suite.
        DatabaseHelpers.EnsureDatabaseIsCreated();

        var context = IntegrationTests.DbContextFactory.CreateDbContext();
        _executorRepository = new ExecutorRepository(context);
        _loggerMock = Substitute.For<ILogger<ExecutorCleaner>>();
        _dateTimeProviderMock = Substitute.For<IDateTimeProvider>();
        _metricsMock = new ExecutorCleanerMetrics(new FakeMeterFactory());

        _executorCleaner = new ExecutorCleaner(_executorRepository, _loggerMock, _dateTimeProviderMock, _metricsMock);
    }

    private IExecutorRepository _executorRepository;
    private ILogger<ExecutorCleaner> _loggerMock;
    private IDateTimeProvider _dateTimeProviderMock;
    private ExecutorCleanerMetrics _metricsMock;
    private ExecutorCleaner _executorCleaner;

    [TestCase(-60, 30, 0, TestName = "WhenJobsOlderThanRetentionDays")]
    [TestCase(-30, 30, 0, TestName = "WhenJobsSameDayAsRetentionDays")]
    [TestCase(-10, 30, 3, TestName = "WhenJobsNewerThanRetentionDays")]
    public async Task Clean_ReturnsExpectedResult(int executionStartDeduction, int retentionDays,
        int expectedResultCount)
    {
        var nowUtc = DateTime.UtcNow;
        _dateTimeProviderMock.NowUtc.Returns(nowUtc);

        await TestAsync(
            async () =>
            {
                var executors = await SampleData.AddExecutorRangeAsync(
                    IntegrationTests.DbContextFactory,
                    () =>
                    [
                        TestData.CreateExecutor("ExecutorOne",
                            deletedUtc: DateTime.UtcNow.AddDays(executionStartDeduction).AddSeconds(-1)),
                        TestData.CreateExecutor("ExecutorTwo",
                            deletedUtc: DateTime.UtcNow.AddDays(executionStartDeduction).AddSeconds(-1)),
                        TestData.CreateExecutor("ExecutorThree",
                            deletedUtc: DateTime.UtcNow.AddDays(executionStartDeduction).AddSeconds(-1))
                    ],
                    default
                );

                return executors.Select(e => e.Id).ToList();
            },
            async (_, token) =>
            {
                await _executorCleaner.CleanAsync(retentionDays, token);
                return Task.CompletedTask;
            },
            async (executorIds, _, ct) =>
            {
                var executors = await _executorRepository.GetByIdsAsync(executorIds, ct);
                executors.Count.Should().Be(expectedResultCount);
            }
        );
    }
}
