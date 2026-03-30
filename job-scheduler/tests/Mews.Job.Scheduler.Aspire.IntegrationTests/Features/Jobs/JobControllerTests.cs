using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Mews.Job.Scheduler.Aspire.IntegrationTests.Helpers;
using Mews.Job.Scheduler.BuildingBlocks.Types.DateTimeSpan;
using Mews.Job.Scheduler.Domain.Jobs;
using Mews.Job.Scheduler.Features;
using Mews.Job.Scheduler.Features.Jobs.Dtos;
using Mews.Job.Scheduler.Tests;
using Microsoft.EntityFrameworkCore;

namespace Mews.Job.Scheduler.Aspire.IntegrationTests.Features.Jobs;

internal sealed class JobControllerTests : TestBase
{
    [Test]
    public async Task Get_ReturnsOKAndCorrectJob()
    {
        await TestAsync(
            arrange: async () =>
            {
                var job = await SampleData.AddJobAsync(IntegrationTests.DbContextFactory, profile => TestData.CreateJob(JobState.Pending, profileId: profile.Id), default);
                return job.Id;
            },
            act: (id, token) =>
            {
                return HttpClientHelpers.AuthorizedHttpClientActionAsync(
                    action: (client, cancellationToken) => client.GetAsync($"/api/jobs/{id}", cancellationToken),
                    cancellationToken: token
                );
            },
            assert: async (id, response, _) =>
            {
                response.EnsureSuccessStatusCode();
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                var responseDto = await SerializationHelpers.DeserializeResponseAsync<JobDto>(response);
                Assert.That(responseDto.Id, Is.EqualTo(id));
            }
        );
    }

    [Test]
    public async Task Filter_ReturnsOKAndCorrectJob()
    {
        await TestAsync(
            arrange: async () =>
            {
                var job = await SampleData.AddJobAsync(IntegrationTests.DbContextFactory, profile => TestData.CreateJob(JobState.Pending, profileId: profile.Id), default);
                return job.Id;
            },
            act: (id, token) =>
            {
                return HttpClientHelpers.AuthorizedHttpClientActionAsync(
                    action: (client, cancellationToken) => client.PostAsJsonAsync($"/api/jobs/filter", new { Ids = new List<Guid>([id]) }, cancellationToken),
                    cancellationToken: token
                );
            },
            assert: async (id, response, _) =>
            {
                response.EnsureSuccessStatusCode();
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                var responseDto = await SerializationHelpers.DeserializeResponseAsync<JobGetResultDto>(response);
                Assert.That(responseDto.Jobs.First().Id, Is.EqualTo(id));
            }
        );
    }

    [Test]
    public async Task Create_ReturnsOKAndCorrectJob()
    {
        await TestAsync(
            arrange: async () =>
            {
                _ = await SampleData.AddExecutorRangeAsync(
                    IntegrationTests.DbContextFactory,
                    create: () => [TestData.CreateExecutor("ExistingExecutor", "Tooling")],
                    cancellationToken: default
                );

                var job = TestData.CreateJob(JobState.Pending);
                var dataWithExistingExecutor = new JobCreateDataDto
                {
                    Name = job.NameNew,
                    ExecutorTypeName = "JobWithExistingExecutor",
                    Team = job.Executor.Team,
                    StartUtc = job.StartUtc,
                    Period = job.Period,
                    MaxExecutionTime = job.MaxExecutionTime,
                    Options = JobOptionsDto.None,
                    Data = job.Data,
                    CreatorProfileId = job.CreatorProfileId
                };
                var dataWithoutExistingExecutor = new JobCreateDataDto
                {
                    Name = job.NameNew,
                    ExecutorTypeName = "JobWithoutExistingExecutor",
                    Team = "Batman",
                    StartUtc = job.StartUtc,
                    Period = job.Period,
                    MaxExecutionTime = job.MaxExecutionTime,
                    Options = JobOptionsDto.None,
                    Data = job.Data,
                    CreatorProfileId = job.CreatorProfileId
                };

                return new List<JobCreateDataDto> { dataWithExistingExecutor, dataWithoutExistingExecutor };
            },
            act: (dto, token) => HttpClientHelpers.AuthorizedHttpClientActionAsync(
                action: (client, cancellationToken) =>
                {
                    var payloadDto = new JobCreateParametersDto
                    {
                        Jobs = dto
                    };
                    var payload = SerializationHelpers.Serialize(payloadDto);
                    return client.PostAsync("/api/jobs", payload, cancellationToken);
                },
                cancellationToken: token
            ),
            assert: async (dto, response, token) =>
            {
                response.EnsureSuccessStatusCode();
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                
                var arrangedData = dto.ToList();
                var executorTypes = arrangedData.Select(d => d.ExecutorTypeName);
                var creatorProfiles = arrangedData.Select(d => d.CreatorProfileId);
                await using var dbContext = IntegrationTests.DbContextFactory.CreateDbContext();

                var jobs = await dbContext.Jobs
                    .Where(j => executorTypes.Contains(j.Executor.Type))
                    .Include(j => j.Executor)
                    .ToListAsync(token);
                jobs.Count.Should().Be(2);
                jobs.Should().OnlyContain(j => executorTypes.Contains(j.Executor.Type));
                jobs.Should().OnlyContain(j => creatorProfiles.Contains(j.CreatorProfileId));

                var executors = await dbContext.Executors.Where(e => executorTypes.Contains(e.Type)).ToListAsync(token);
                var arrangedExecutorTeams = arrangedData.Select(d => d.Team);
                executors.Count.Should().Be(arrangedData.Count);
                executors.Should().OnlyContain(e => arrangedExecutorTeams.Contains(e.Team));
                executors.Should().OnlyContain(e => e.DeletedUtc == null);
            }
        );
    }

    [Test]
    public async Task Update_ReturnsOKAndUpdatedJob()
    {
        await TestAsync(
            arrange: async () =>
            {
                var executorToUpdateTo = await SampleData.AddExecutorAsync(IntegrationTests.DbContextFactory, () => TestData.CreateExecutor("Updated", "Tooling"), default);
                var job = await SampleData.AddJobAsync(IntegrationTests.DbContextFactory, profile => TestData.CreateJob(JobState.Pending, profileId: profile.Id), default);
                var updatedJobData = new JobUpdateDataDto
                {
                    Name = "Updated",
                    ExecutorTypeName = executorToUpdateTo.Type,
                    Team = executorToUpdateTo.Team,
                    StartUtc = job.StartUtc.AddDays(1),
                    Period = job.Period,
                    MaxExecutionTime = job.MaxExecutionTime.Add(new DateTimeSpan(days: 1)),
                    Options = ApiDtoMapper.Convert<JobOptions, JobOptionsDto>(JobOptions.IsFatal | JobOptions.ParallelExecution),
                    Data = job.Data,
                    UpdaterProfileId = Guid.NewGuid()
                };

                var payloadDto = new JobUpdateParametersDto
                {
                    UpdatedJob = updatedJobData
                };

                return (JobId: job.Id, UpdateDto: payloadDto);
            },
            act: (data, token) => HttpClientHelpers.AuthorizedHttpClientActionAsync(
                action: (client, cancellationToken) =>
                {
                    var payload = SerializationHelpers.Serialize(data.UpdateDto);
                    return client.PutAsync($"/api/jobs/{data.JobId}", payload, cancellationToken);
                },
                cancellationToken: token
            ),
            assert: async (data, response, token) =>
            {
                response.EnsureSuccessStatusCode();
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                
                await using var dbContext = IntegrationTests.DbContextFactory.CreateDbContext();
                var job = await dbContext.Jobs
                    .Where(j => j.Id == data.JobId)
                    .Include(j => j.Executor)
                    .SingleOrDefaultAsync(token);
                job.Should().NotBeNull();
                    
                var jobUpdateDto = data.UpdateDto.UpdatedJob;
                job!.NameNew.Should().Be(jobUpdateDto.Name);
                job.Executor.Type.Should().Be(jobUpdateDto.ExecutorTypeName);
                job.Executor.Team.Should().Be(jobUpdateDto.Team);
                job.StartUtc.Should().Be(jobUpdateDto.StartUtc);
                job.Period.Should().Be(jobUpdateDto.Period);
                job.MaxExecutionTime.Should().Be(jobUpdateDto.MaxExecutionTime);
                job.Options.Should().Be(ApiDtoMapper.Convert<JobOptionsDto, JobOptions>(jobUpdateDto.Options));
                job.Data.Should().Be(jobUpdateDto.Data);
                job.UpdaterProfileId.Should().Be(jobUpdateDto.UpdaterProfileId);
            }
        );
    }

    [Test]
    public async Task Delete_ReturnsNoContentAndJobIsSoftDeleted()
    {
        await TestAsync(
            arrange: async () =>
            {
                var job = await SampleData.AddJobAsync(IntegrationTests.DbContextFactory, profile => TestData.CreateJob(JobState.Pending, profileId: profile.Id), default);
                var updaterProfileId = Guid.NewGuid();
                var dto = new JobDeleteParametersDto
                {
                    UpdaterProfileId = updaterProfileId
                };

                return (JobId: job.Id, ProfileId: updaterProfileId, Dto: dto);
            },
            act: (data, token) => HttpClientHelpers.AuthorizedHttpClientActionAsync(
                action: async (client, cancellationToken) =>
                {
                    var payload = SerializationHelpers.Serialize(data.Dto);
                    return await client.DeleteAsync($"/api/jobs/{data.JobId}", payload, cancellationToken);
                },
                cancellationToken: token
            ),
            assert: async (data, response, token) =>
            {
                response.EnsureSuccessStatusCode();
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
                await AssertHelpers.JobThatAsync(IntegrationTests.DbContextFactory, data.JobId, token: token, assert: job =>
                {
                    Assert.That(job.IsDeleted, Is.True);
                    Assert.That(job.DeletedUtc, Is.Not.Null);
                    Assert.That(job.UpdaterProfileId, Is.EqualTo(data.ProfileId));
                });
            }
        );
    }
}
