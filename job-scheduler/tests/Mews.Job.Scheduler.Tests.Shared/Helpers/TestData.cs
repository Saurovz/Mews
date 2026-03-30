using Mews.Job.Scheduler.BuildingBlocks.Types.DateTimeSpan;
using Mews.Job.Scheduler.Domain.Executors;
using Mews.Job.Scheduler.Domain.JobExecutions;
using Mews.Job.Scheduler.Domain.Jobs;

namespace Mews.Job.Scheduler.Tests;

public static class TestData
{
    public static Domain.Jobs.Job CreateJob(
        JobState state,
        Executor? executor = null,
        string? teamName = null,
        JobOptions options = JobOptions.None,
        DateTime? createdUtc = null,
        DateTime? startUtc = null,
        DateTimeSpan? period = null,
        bool isDeleted = false,
        Guid? profileId = null)
    {
        
        var team = teamName ?? $"{Guid.NewGuid()}";
        executor ??= CreateExecutor(Guid.NewGuid().ToString(), team);
        var timestamp = createdUtc ?? DateTime.UtcNow.AddDays(-1);
        var profile = profileId ?? Guid.NewGuid();

        return new Domain.Jobs.Job
        {
            Id = Guid.NewGuid(),
            ExecutorId = executor.Id,
            Executor = executor,
            State = state,
            StartUtc = startUtc ?? timestamp,
            Options = options,
            Period = period,
            CreatedUtc = timestamp,
            UpdatedUtc = timestamp,
            CreatorProfileId = profile,
            UpdaterProfileId = profile,
            IsDeleted = state is JobState.Executed || isDeleted,
            MaxExecutionTime = new DateTimeSpan(minutes: 30),
            ExecutionStartUtc = timestamp
        };
    }

    public static JobExecution CreateJobExecution(
        JobExecutionState state,
        Domain.Jobs.Job? job = null,
        DateTime? createdUtc = null,
        string? transactionIdentifier = null,
        bool isDeleted = false)
    {
        var timestamp = job?.ExecutionStartUtc ?? createdUtc ?? DateTime.UtcNow.AddDays(-1);
        var endUtc = state is not JobExecutionState.InProgress
            ? DateTime.UtcNow
            : (DateTime?)null;

        return new JobExecution
        {
            Id = Guid.NewGuid(),
            JobId = job?.Id ?? Guid.NewGuid(),
            State = state,
            StartUtc = timestamp,
            EndUtc = endUtc,
            ExecutorTypeNameValue = job?.Executor.Type ?? "TestJobExecutor",
            CreatedUtc = timestamp,
            UpdatedUtc = timestamp,
            CreatorProfileId = Guid.NewGuid(),
            UpdaterProfileId = Guid.NewGuid(),
            TransactionIdentifier = transactionIdentifier,
            IsDeleted = isDeleted,
        };
    }

    public static Executor CreateExecutor(string type, string? team = null, DateTime? deletedUtc = null)
    {
        return new Executor
        {
            Id = Guid.NewGuid(),
            Type = type,
            Team = team ?? "Tooling",
            DeletedUtc = deletedUtc
        };
    }
}
