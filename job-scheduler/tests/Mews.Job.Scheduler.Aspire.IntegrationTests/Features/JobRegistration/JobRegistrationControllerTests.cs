using System.Net;
using FluentAssertions;
using Mews.Job.Scheduler.Aspire.IntegrationTests.Helpers;
using Mews.Job.Scheduler.Core.EntityFrameworkCore;
using Mews.Job.Scheduler.Domain.Executors;
using Mews.Job.Scheduler.Domain.Jobs;
using Mews.Job.Scheduler.Features;
using Mews.Job.Scheduler.Features.Jobs.Dtos;
using Mews.Job.Scheduler.Tests;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

namespace Mews.Job.Scheduler.Aspire.IntegrationTests.Features.JobRegistration;

[TestFixture]
internal sealed class JobRegistrationControllerTests : TestBase
{
    [SetUp]
    public void SetUp()
    {
        // Explicitly clear database and data from other tests, the tests were deleting jobs from other tests in the suite.
        DatabaseHelpers.EnsureDatabaseIsCreated();
    }

    [Test]
    public async Task Register_ReturnsOKAndCorrectJobsWithExecutors()
    {
        await TestAsync(
            arrange: async () =>
            {
                var jobToKeepExecutor = TestData.CreateExecutor("JobToKeep", "Tooling");
                var jobToKeepAndRegisterExecutor = TestData.CreateExecutor("jobToKeepAndRegister", "Tooling");
                var jobToKeepAndRegisterTwoExecutor = TestData.CreateExecutor("jobToKeepAndRegisterTwo", "Tooling");
                var jobToDeleteExecutor = TestData.CreateExecutor("JobToDelete", "Tooling");
                var jobToRegisterExecutor = TestData.CreateExecutor("JobToRegister", "Tooling");

                var jobToKeep = TestData.CreateJob(JobState.Pending, jobToKeepExecutor);
                var jobToKeepAndRegister = TestData.CreateJob(JobState.Pending, jobToKeepAndRegisterExecutor);
                var jobToKeepAndRegisterTwo = TestData.CreateJob(JobState.Pending, jobToKeepAndRegisterTwoExecutor);
                var jobToDelete = TestData.CreateJob(JobState.Pending, jobToDeleteExecutor);
                var jobToRegister = TestData.CreateJob(JobState.Pending, jobToRegisterExecutor);

                _ = await SampleData.AddJobRangeAsync(
                    IntegrationTests.DbContextFactory,
                    create: _ => [jobToKeep, jobToKeepAndRegister, jobToKeepAndRegisterTwo, jobToDelete],
                    cancellationToken: default
                );
                var recognizedJobs = new[] { jobToKeep, jobToKeepAndRegister, jobToKeepAndRegisterTwo, jobToRegister };
                var recognizedJobExecutors = recognizedJobs.Select(j => new JobExecutorMetadataDto
                {
                    Type = j.Executor.Type,
                    Team = j.Executor.Team
                });

                var jobsToRegister = new[]
                {
                    jobToKeepAndRegister,
                    jobToKeepAndRegisterTwo,
                    jobToRegister
                };
                var dto = new JobRegistrationParametersDto
                {
                    RecognizedJobExecutorsMetadata = recognizedJobExecutors,
                    JobsToRegister = jobsToRegister.Select(Serialize),
                    UpdaterProfileId = Guid.NewGuid()
                };

                return (TypeToAdd: jobToRegister.Executor.Type, TypeToDelete: jobToDelete.Executor.Type, Dto: dto);
            },
            act: (data, token) => HttpClientHelpers.AuthorizedHttpClientActionAsync(
                action: (client, cancellationToken) =>
                {
                    var payload = SerializationHelpers.Serialize(data.Dto);
                    return client.PostAsync("/api/registration", payload, cancellationToken);
                },
                cancellationToken: token
            ),
            assert: async (data, response, token) =>
            {
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var responseDto = await SerializationHelpers.DeserializeResponseAsync<JobRegistrationResultDto>(response);
                responseDto.CreatedJobs.Should().OnlyContain(j => j.ExecutorTypeName == data.TypeToAdd);
                responseDto.DeletedJobs.Should().OnlyContain(j => j.ExecutorTypeName == data.TypeToDelete);

                await using var dbContext = IntegrationTests.DbContextFactory.CreateDbContext();
                var jobs = await dbContext.Jobs.Where(j => j.Executor.Type == data.TypeToDelete).ToListAsync(token);
                jobs.Should().OnlyContain(j => j.IsDeleted);
            }
        );
    }

    private JobCreateDataDto Serialize(Domain.Jobs.Job job)
    {
        return new JobCreateDataDto
        {
            Name = job.NameNew,
            ExecutorTypeName = job.Executor.Type,
            Team = job.Executor.Team,
            StartUtc = job.StartUtc,
            Period = job.Period,
            MaxExecutionTime = job.MaxExecutionTime,
            Options = ApiDtoMapper.Convert<JobOptions, JobOptionsDto>(job.Options),
            Data = job.Data,
            CreatorProfileId = job.CreatorProfileId
        };
    }
}
