using Mews.Job.Scheduler.Domain.Jobs;

namespace Mews.Job.Scheduler.Features;

public static class ApiDtoMapper
{
    public static TTargetEnum Convert<TSourceEnum, TTargetEnum>(TSourceEnum value)
        where TSourceEnum : struct, Enum
        where TTargetEnum : struct, Enum
    {
        return Convert<TSourceEnum, TTargetEnum>((TSourceEnum?)value)!.Value;
    }

    public static TTargetEnum? Convert<TSourceEnum, TTargetEnum>(TSourceEnum? value)
        where TSourceEnum : struct, Enum
        where TTargetEnum : struct, Enum
    {
        return value is { } v ? (TTargetEnum)(object)v : null;
    }

    public static Jobs.Dtos.JobDto ToJobDto(Domain.Jobs.Job job)
    {
        return new Jobs.Dtos.JobDto
        {
            Id = job.Id,
            Name = job.NameNew,
            ExecutorTypeName = job.Executor.Type,
            Team = job.Executor.Team,
            StartUtc = job.StartUtc,
            PreviousSuccessfulStartUtc = job.PreviousSuccessfulStartUtc,
            Period = job.Period,
            MaxExecutionTime = job.MaxExecutionTime,
            ExecutionStartUtc = job.ExecutionStartUtc,
            Status = Convert<JobState, Jobs.Dtos.JobStateDto>(job.State),
            Options = Convert<JobOptions, Jobs.Dtos.JobOptionsDto>(job.Options),
            Data = job.Data,
            CreatedUtc = job.CreatedUtc,
            CreatorProfileId = job.CreatorProfileId,
            UpdatedUtc = job.UpdatedUtc,
            UpdaterProfileId = job.UpdaterProfileId,
            IsDeleted = job.IsDeleted,
            DeletedUtc = job.DeletedUtc
        };
    }
}
