using System.Collections.Immutable;
using FluentAssertions;
using Mews.Atlas.FeatureFlags;
using Mews.Job.Scheduler.Aspire.IntegrationTests.Helpers;
using Mews.Job.Scheduler.BuildingBlocks.Types.DateTimeSpan;
using Mews.Job.Scheduler.Core.Application.JobRegistration.Handlers;
using Mews.Job.Scheduler.Core.Application.Registration;
using Mews.Job.Scheduler.Core.EntityFrameworkCore.Executors;
using Mews.Job.Scheduler.Core.EntityFrameworkCore.JobExecutions;
using Mews.Job.Scheduler.Core.EntityFrameworkCore.JobPersistence;
using Mews.Job.Scheduler.Core.EntityFrameworkCore.Jobs;
using Mews.Job.Scheduler.Domain;
using Mews.Job.Scheduler.Domain.DateTimeProviders;
using Mews.Job.Scheduler.Domain.Executors;
using Mews.Job.Scheduler.Domain.Jobs;
using Mews.Job.Scheduler.Tests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Mews.Job.Scheduler.Aspire.IntegrationTests.Features.JobRegistration;

[TestFixture]
internal sealed class JobRegistrationHandlerTests : TestBase
{
    private IDateTimeProvider _dateTimeProviderMock;
    private IExecutorRepository _executorRepository;
    private IJobPersistence _jobPersistence;
    private ILogger<JobRegistrationHandler> _loggerMock;
    private JobRegistrationHandler _handler;

    [SetUp]
    public void SetUp()
    {
        // Explicitly clear database and data from other tests, the tests were deleting jobs from other tests in the suite.
        DatabaseHelpers.EnsureDatabaseIsCreated();

        _loggerMock = Substitute.For<ILogger<JobRegistrationHandler>>();
        _dateTimeProviderMock = Substitute.For<IDateTimeProvider>();
        var context = IntegrationTests.DbContextFactory.CreateDbContext();
        var jobRepository = new JobRepository(context);

        var jobExecutionRepository = new JobExecutionRepository(context);
        _jobPersistence = new JobPersistence(context, jobRepository, jobExecutionRepository);
        _executorRepository = new ExecutorRepository(context);

        _handler = new JobRegistrationHandler(
            _dateTimeProviderMock,
            _jobPersistence,
            _executorRepository,
            _loggerMock);
    }

    [Test]
    public async Task Register_ReturnsCorrectCreatedAndDeletedJobsWithExecutors()
    {
        var nowUtc = DateTime.UtcNow;
        _dateTimeProviderMock.NowUtc.Returns(nowUtc);

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
                var recognizedJobExecutors = recognizedJobs.Select(j => new ExecutorCreateParameters(j.Executor.Type, j.Executor.Team));

                var jobsToRegister = new[]
                {
                    jobToKeepAndRegister,
                    jobToKeepAndRegisterTwo,
                    jobToRegister
                };

                var registrationCommand = new JobRegistrationCommand
                {
                    RecognizedExecutorsMetadata = recognizedJobExecutors.ToList(),
                    JobsToRegister = jobsToRegister.Select(Serialize).ToList(),
                    UpdaterProfileId = Guid.NewGuid()
                };

                return (
                    ExecutorTypeToAdd: jobToRegisterExecutor.Type,
                    ExecutorIdToDelete: jobToDelete.ExecutorId,
                    JobRegistrationCommand: registrationCommand
                );
            },
            act: async (data, token) =>
            {
                var result = await _handler.Handle(data.JobRegistrationCommand, token);
                return result;
            },
            assert: async (data, result, token) =>
            {
                result.CreatedJobs.Should().OnlyContain(j => j.Executor.Type == data.ExecutorTypeToAdd);
                result.DeletedJobs.Should().OnlyContain(j => j.ExecutorId == data.ExecutorIdToDelete);

                await using var dbContext1 = IntegrationTests.DbContextFactory.CreateDbContext();
                var executors = await dbContext1.Executors
                    .Include(e => e.Jobs)
                    .ToListAsync(token);

                var types = executors.Select(e => e.Type);
                data.JobRegistrationCommand.RecognizedExecutorsMetadata.Should().OnlyContain(m => types.Contains(m.Type));

                var deletedExecutor = executors.Single(e => e.Id == data.ExecutorIdToDelete);
                deletedExecutor.DeletedUtc.Should().NotBeNull();
                deletedExecutor.Jobs.Should().OnlyContain(j => j.IsDeleted);

                var addedExecutor = executors.Single(e => e.Type == data.ExecutorTypeToAdd);
                var addedJobIds = result.CreatedJobs.Select(j => j.Id);
                addedExecutor.Jobs.Should().OnlyContain(j => addedJobIds.Contains(j.Id));

                // Revert changes
                await using var dbContext2 = IntegrationTests.DbContextFactory.CreateDbContext();
                dbContext2.Jobs.RemoveRange(executors.SelectMany(e => e.Jobs));
                dbContext2.Executors.RemoveRange(executors);
                await dbContext2.SaveChangesAsync(token);
            });
    }

    [Test]
    public async Task Register_RestoresDeletedExecutorWhenRecognizedExecutorIsPresent()
    {
        await TestAsync(
            arrange: async () =>
            {
                var executorToRestore = await SampleData.AddExecutorAsync(IntegrationTests.DbContextFactory, () => TestData.CreateExecutor("ExecutorToRestore", "Tooling", DateTime.UtcNow), default);
                var jobCreateParameters = GetCreateParameters(executorToRestore.Type);

                return new JobRegistrationCommand
                {
                    RecognizedExecutorsMetadata = new List<ExecutorCreateParameters> { new ExecutorCreateParameters(executorToRestore.Type, executorToRestore.Team) },
                    JobsToRegister = new List<JobCreateParameters> { jobCreateParameters },
                    UpdaterProfileId = Guid.NewGuid()
                };
            },
            act: async (command, token) =>
            {
                var result = await _handler.Handle(command, token);
                return result;
            },
            assert: async (command, _, token) =>
            {
                await using var dbContext1 = IntegrationTests.DbContextFactory.CreateDbContext();
                var executor = await dbContext1.Executors
                    .Where(e => e.Type == command.RecognizedExecutorsMetadata.Single().Type)
                    .Include(e => e.Jobs)
                    .SingleAsync(token);
                executor.DeletedUtc.Should().BeNull();
                executor.Jobs.Count.Should().Be(1);

                // Revert changes
                await using var dbContext2 = IntegrationTests.DbContextFactory.CreateDbContext();
                dbContext2.Jobs.RemoveRange(executor.Jobs);
                dbContext2.Executors.Remove(executor);
                await dbContext2.SaveChangesAsync(token);
            }
        );
    }

    [Test]
    public async Task Register_DoesNotRegisterNewJobWhenExecutorAlreadyExists()
    {
        await TestAsync(
            arrange: async () =>
            {
                var executorToRestore = await SampleData.AddExecutorAsync(IntegrationTests.DbContextFactory, () => TestData.CreateExecutor("ExistingExecutor", "Tooling"), default);
                var jobCreateParameters = GetCreateParameters(executorToRestore.Type);

                return new
                {
                    ExecutorToRestore = executorToRestore,
                    JobRegistrationCommand = new JobRegistrationCommand
                    {
                        RecognizedExecutorsMetadata = new List<ExecutorCreateParameters> { new ExecutorCreateParameters(executorToRestore.Type, executorToRestore.Team) },
                        JobsToRegister = new List<JobCreateParameters> { jobCreateParameters },
                        UpdaterProfileId = Guid.NewGuid()
                    }
                };
            },
            act: async (arrangedData, token) =>
            {
                var result = await _handler.Handle(arrangedData.JobRegistrationCommand, token);
                return result;
            },
            assert: async (arrangedData, result, token) =>
            {
                result.CreatedJobs.Should().BeEmpty();
                result.DeletedJobs.Should().BeEmpty();

                // Revert changes
                await using var dbContext = IntegrationTests.DbContextFactory.CreateDbContext();
                dbContext.Executors.Remove(arrangedData.ExecutorToRestore);
                await dbContext.SaveChangesAsync(token);
            }
        );
    }

    [Test]
    public async Task Register_SynchronizesTeamWhenChanged()
    {
        await TestAsync(
            arrange: async () =>
            {
                var updatedTeamName = "ToolingUpdated";
                var executorToUpdate = await SampleData.AddExecutorAsync(IntegrationTests.DbContextFactory, () => TestData.CreateExecutor("ExecutorToUpdate", "Tooling"), default);
                var executorToRemain = await SampleData.AddExecutorAsync(IntegrationTests.DbContextFactory, () => TestData.CreateExecutor("ExecutorToRemain", "Tooling"), default);
                var executorCreateParameters = new List<ExecutorCreateParameters>
                {
                    new(executorToUpdate.Type, updatedTeamName),
                    new(executorToRemain.Type, executorToRemain.Team)
                };
                var jobsToRegister = new List<JobCreateParameters>
                {
                    GetCreateParameters(executorToUpdate.Type),
                    GetCreateParameters(executorToRemain.Type)
                };

                return new
                {
                    UpdatedTeamName = updatedTeamName,
                    ExecutorTypeToUpdate = executorToUpdate.Type,
                    ExecutorTypeToRemain = executorToRemain.Type,
                    JobRegistrationCommand = new JobRegistrationCommand
                    {
                        RecognizedExecutorsMetadata = executorCreateParameters,
                        JobsToRegister = jobsToRegister,
                        UpdaterProfileId = Guid.NewGuid()
                    }
                };
            },
            act: async (arrangedData, token) =>
            {
                var result = await _handler.Handle(arrangedData.JobRegistrationCommand, token);
                return result;
            },
            assert: async (arrangedData, result, token) =>
            {
                await using var dbContext = IntegrationTests.DbContextFactory.CreateDbContext();
                var executorNamesToFetch = arrangedData.JobRegistrationCommand.RecognizedExecutorsMetadata.Select(m => m.Type);
                var executors = await dbContext.Executors
                    .Where(e => executorNamesToFetch.Contains(e.Type))
                    .ToListAsync(token);

                var updatedExecutor = executors.Single(e => e.Type == arrangedData.ExecutorTypeToUpdate);
                updatedExecutor.Team.Should().Be(arrangedData.UpdatedTeamName);
                
                var remainingExecutor = executors.Single(e => e.Type == arrangedData.ExecutorTypeToRemain);
                remainingExecutor.Team.Should().Be(remainingExecutor.Team);

                // Revert changes
                dbContext.Executors.RemoveRange(executors);
                await dbContext.SaveChangesAsync(token);
            }
        );
    }

    private JobCreateParameters GetCreateParameters(string executorTypeName)
    {
        return new JobCreateParameters(
            startUtc: DateTime.UtcNow,
            executorTypeName: executorTypeName,
            team: "Tooling",
            maxExecutionTime: new DateTimeSpan(minutes: 15),
            creatorProfileId: Guid.NewGuid(),
            name: "Testing",
            period: new DateTimeSpan(minutes: 15),
            options: JobOptions.None,
            data: null
        );
    }

    private JobCreateParameters Serialize(Domain.Jobs.Job job)
    {
        return new JobCreateParameters(
            startUtc: job.StartUtc,
            executorTypeName: job.Executor.Type,
            team: job.Executor.Team,
            maxExecutionTime: job.MaxExecutionTime,
            creatorProfileId: job.CreatorProfileId,
            name: job.NameNew,
            period: job.Period,
            options: job.Options,
            data: job.Data
        );
    }
}
