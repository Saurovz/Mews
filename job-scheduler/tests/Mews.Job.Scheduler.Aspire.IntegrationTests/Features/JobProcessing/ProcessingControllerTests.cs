using System.Net;
using FluentAssertions;
using Mews.Job.Scheduler.Aspire.IntegrationTests.Helpers;
using Mews.Job.Scheduler.BuildingBlocks.Types.DateTimeSpan;
using Mews.Job.Scheduler.Domain.JobExecutions;
using Mews.Job.Scheduler.Domain.Jobs;
using Mews.Job.Scheduler.Features;
using Mews.Job.Scheduler.Features.Jobs.Dtos;
using Mews.Job.Scheduler.Tests;
using Microsoft.EntityFrameworkCore;

namespace Mews.Job.Scheduler.Aspire.IntegrationTests.Features.JobProcessing;

[TestFixture]
internal sealed class ProcessingControllerTests : TestBase
{
    [Test]
    public async Task ConfirmProcessing_ReturnsExpectedResult_WhenJobSetInProgress()
    {
        await TestAsync(
            async () =>
            {
                var job = await SampleData.AddJobAsync(IntegrationTests.DbContextFactory,
                    profile => TestData.CreateJob(JobState.Scheduled, profileId: profile.Id), default);
                return new
                {
                    JobId = job.Id,
                    TransactionIdentifier = Guid.NewGuid().ToString().Replace("-", string.Empty)
                };
            },
            (arrangeData, token) => HttpClientHelpers.AuthorizedHttpClientActionAsync(
                (client, cancellationToken) => client.PostAsync("/api/processing/confirmProcessing",
                    SerializationHelpers.Serialize(arrangeData), cancellationToken),
                token
            ),
            async (arrangeData, response, ct) =>
            {
                response.EnsureSuccessStatusCode();
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                var responseDto =
                    await SerializationHelpers.DeserializeResponseAsync<ConfirmProcessingResultDto>(response);

                await using var dbContext = IntegrationTests.DbContextFactory.CreateDbContext();
                var jobExecution = await dbContext.JobExecutions.Where(e => e.Id == responseDto.JobExecutionId)
                    .SingleOrDefaultAsync(ct);
                Assert.That(jobExecution, Is.Not.Null);
                Assert.That(jobExecution.JobId, Is.EqualTo(arrangeData.JobId));
                Assert.That(jobExecution.State, Is.EqualTo(JobExecutionState.InProgress));
                Assert.That(jobExecution.TransactionIdentifier, Is.EqualTo(arrangeData.TransactionIdentifier));

                var job = await dbContext.Jobs.Where(j => j.Id == jobExecution.JobId).Include(j => j.Executor)
                    .SingleOrDefaultAsync(ct);
                Assert.That(job, Is.Not.Null);
                Assert.That(job.State, Is.EqualTo(JobState.InProgress));
                Assert.That(job.ExecutionStartUtc, Is.Not.Null);

                Assert.That(jobExecution.ExecutorTypeNameValue, Is.EqualTo(job.Executor.Type));
                Assert.That(jobExecution.StartUtc, Is.EqualTo(job.ExecutionStartUtc));

                Assert.That(responseDto.JobExecutionId, Is.EqualTo(jobExecution.Id));
                Assert.That(responseDto.ExecutionStartUtc, Is.EqualTo(jobExecution.StartUtc));
            }
        );
    }

    [Test]
    public async Task ConfirmProcessing_ReturnsExpectedResult_WhenJobWasSetInProgress()
    {
        await TestAsync(
            async () =>
            {
                var transactionIdentifier = Guid.NewGuid().ToString().Replace("-", string.Empty);
                var job = await SampleData.AddJobAsync(IntegrationTests.DbContextFactory,
                    profile => TestData.CreateJob(JobState.InProgress, profileId: profile.Id), default);
                await SampleData.AddJobExecutionAsync(IntegrationTests.DbContextFactory,
                    _ => TestData.CreateJobExecution(JobExecutionState.InProgress, job,
                        transactionIdentifier: transactionIdentifier), default);

                return new
                {
                    JobId = job.Id,
                    TransactionIdentifier = transactionIdentifier
                };
            },
            (arrangeData, token) => HttpClientHelpers.AuthorizedHttpClientActionAsync(
                (client, cancellationToken) => client.PostAsync("/api/processing/confirmProcessing",
                    SerializationHelpers.Serialize(arrangeData), cancellationToken),
                token
            ),
            async (arrangeData, response, ct) =>
            {
                response.EnsureSuccessStatusCode();
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                var responseDto =
                    await SerializationHelpers.DeserializeResponseAsync<ConfirmProcessingResultDto>(response);

                await using var dbContext = IntegrationTests.DbContextFactory.CreateDbContext();
                var jobExecution = await dbContext.JobExecutions
                    .Where(e => e.Id == responseDto.JobExecutionId)
                    .SingleOrDefaultAsync(ct);
                jobExecution.Should().NotBeNull();
                jobExecution!.JobId.Should().Be(arrangeData.JobId);
                jobExecution.State.Should().Be(JobExecutionState.InProgress);
                jobExecution.TransactionIdentifier.Should().Be(arrangeData.TransactionIdentifier);

                var job = await dbContext.Jobs
                    .Where(j => j.Id == jobExecution.JobId)
                    .Include(j => j.Executor)
                    .SingleOrDefaultAsync(ct);
                jobExecution.Should().NotBeNull();
                job!.State.Should().Be(JobState.InProgress);
                job.ExecutionStartUtc.Should().NotBeNull();

                jobExecution.ExecutorTypeNameValue.Should().Be(job.Executor.Type);
                jobExecution.StartUtc.Should().Be(job.ExecutionStartUtc);

                responseDto.JobExecutionId.Should().Be(jobExecution.Id);
                responseDto.ExecutionStartUtc.Should().Be(jobExecution.StartUtc);
            }
        );
    }

    [Test]
    public async Task ConfirmResult_ReturnsOk_WhenJobExecutionIsInProgressAndJobPeriodical()
    {
        await TestAsync(
            async () =>
            {
                var job = await SampleData.AddJobAsync(IntegrationTests.DbContextFactory,
                    profile => TestData.CreateJob(JobState.InProgress, period: new DateTimeSpan(minutes: 10),
                        profileId: profile.Id), default);
                var jobExecution = await SampleData.AddJobExecutionAsync(IntegrationTests.DbContextFactory,
                    _ => TestData.CreateJobExecution(JobExecutionState.InProgress, job), default);

                return new
                {
                    JobExecutionId = jobExecution.Id,
                    Parameters = new
                    {
                        State = ApiDtoMapper.Convert<JobExecutionState, JobExecutionStateDto>(JobExecutionState.Success),
                        Tag = "tag",
                        DeleteJob = false,
                        FutureRunData = "The quick brown fox jumps over the lazy dog"
                    }
                };
            },
            (arrangeData, token) => HttpClientHelpers.AuthorizedHttpClientActionAsync(
                (client, cancellationToken) => client.PostAsync("/api/processing/confirmResult",
                    SerializationHelpers.Serialize(arrangeData), cancellationToken),
                token
            ),
            async (arrangeData, response, ct) =>
            {
                response.EnsureSuccessStatusCode();
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

                await using var dbContext = IntegrationTests.DbContextFactory.CreateDbContext();
                var jobExecution = await dbContext.JobExecutions.FindAsync(arrangeData.JobExecutionId, ct);
                Assert.That(jobExecution, Is.Not.Null);
                Assert.That(jobExecution.State, Is.EqualTo(JobExecutionState.Success));
                Assert.That(jobExecution.Tag, Is.EqualTo(arrangeData.Parameters.Tag));

                var job = await dbContext.Jobs.FindAsync(jobExecution.JobId, ct);
                Assert.That(job, Is.Not.Null);
                Assert.That(job.State, Is.EqualTo(JobState.Pending));
                Assert.That(job.ExecutionStartUtc, Is.Null);
            }
        );
    }

    [Test]
    public async Task ConfirmResult_ReturnsOk_WhenJobExecutionIsInProgressAndJobNonPeriodical()
    {
        await TestAsync(
            async () =>
            {
                var job = await SampleData.AddJobAsync(IntegrationTests.DbContextFactory,
                    profile => TestData.CreateJob(JobState.InProgress, profileId: profile.Id), default);
                var jobExecution = await SampleData.AddJobExecutionAsync(IntegrationTests.DbContextFactory,
                    _ => TestData.CreateJobExecution(JobExecutionState.InProgress, job), default);

                return new
                {
                    JobExecutionId = jobExecution.Id,
                    Parameters = new
                    {
                        State =
                            ApiDtoMapper.Convert<JobExecutionState, JobExecutionStateDto>(JobExecutionState.Success),
                        Tag = "tag",
                        DeleteJob = false,
                        FutureRunData = "The quick brown fox jumps over the lazy dog"
                    }
                };
            },
            (arrangeData, token) => HttpClientHelpers.AuthorizedHttpClientActionAsync(
                (client, cancellationToken) => client.PostAsync("/api/processing/confirmResult",
                    SerializationHelpers.Serialize(arrangeData), cancellationToken),
                token
            ),
            async (arrangeData, response, ct) =>
            {
                response.EnsureSuccessStatusCode();
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

                await using var dbContext = IntegrationTests.DbContextFactory.CreateDbContext();

                var jobExecution = await dbContext.JobExecutions.FindAsync(arrangeData.JobExecutionId, ct);
                Assert.That(jobExecution, Is.Not.Null);
                Assert.That(jobExecution.State, Is.EqualTo(JobExecutionState.Success));
                Assert.That(jobExecution.Tag, Is.EqualTo(arrangeData.Parameters.Tag));

                var job = await dbContext.Jobs.FindAsync(jobExecution.JobId, ct);
                Assert.That(job, Is.Not.Null);
                Assert.That(job.State, Is.EqualTo(JobState.Executed));
                Assert.That(job.IsDeleted, Is.True);
            }
        );
    }
}
