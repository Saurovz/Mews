using System.Net;
using System.Net.Http.Json;
using Mews.Job.Scheduler.Aspire.IntegrationTests.Helpers;
using Mews.Job.Scheduler.Domain.JobExecutions;
using Mews.Job.Scheduler.Domain.Jobs;
using Mews.Job.Scheduler.Features.Jobs.Dtos;
using Mews.Job.Scheduler.Tests;

namespace Mews.Job.Scheduler.Aspire.IntegrationTests.Features.JobExecutions;

[TestFixture]
internal sealed class JobExecutionControllerTests : TestBase
{
    [Test]
    public async Task Get_ReturnsOkAndJobExecution_WhenFiltersAreProvided()
    {
        await TestAsync(
            arrange: async () =>
            {
                var job = await SampleData.AddJobAsync(IntegrationTests.DbContextFactory, _ => TestData.CreateJob(JobState.Pending), default);
                var job2 = await SampleData.AddJobAsync(IntegrationTests.DbContextFactory, _ => TestData.CreateJob(JobState.InProgress), default);
                await SampleData.AddJobExecutionRangeAsync(IntegrationTests.DbContextFactory, _ =>
                [
                    TestData.CreateJobExecution(JobExecutionState.Success, job: job),
                    TestData.CreateJobExecution(JobExecutionState.InProgress, job: job, isDeleted: true),
                    TestData.CreateJobExecution(JobExecutionState.Timeout, job: job),
                    TestData.CreateJobExecution(JobExecutionState.InProgress, job: job2)
                ], 
                default);

                return new
                {
                    JobIds = new[] { job.Id },
                    States = new[] { "InProgress", "Success" },
                    StatesNew = new[] { "InProgress", "Success" },
                    ShowDeleted = true,
                    Limitation = new
                    {
                        Count = 10,
                        StartIndex = 0
                    }
                    
                };
            },
            act: (arrangeData, token) => HttpClientHelpers.AuthorizedHttpClientActionAsync(
                action: (client, cancellationToken) =>
                {
                    var requestMessage = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        Content = JsonContent.Create(arrangeData),
                        RequestUri = new Uri("/api/jobExecutions", UriKind.RelativeOrAbsolute)
                    };
                    return client.SendAsync(requestMessage, cancellationToken);
                },
                cancellationToken: token
            ),
            assert: async (arrangeData, response, _) =>
            {
                response.EnsureSuccessStatusCode();
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                var responseDto = await SerializationHelpers.DeserializeResponseAsync<JobExecutionGetResultDto>(response);
                Assert.That(responseDto.JobExecutions.Count(), Is.EqualTo(2));
                
                Assert.That(arrangeData.JobIds, Is.EquivalentTo(responseDto.JobExecutions.Select(e => e.Job.Id).Distinct()));
                Assert.That(new [] { JobExecutionStateDto.InProgress, JobExecutionStateDto.Success }, Is.EquivalentTo(responseDto.JobExecutions.Select(e => e.State)));
            }
        );
    }
}
