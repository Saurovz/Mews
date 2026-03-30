using Mews.Job.Scheduler.BuildingBlocks.Domain;
using Mews.Job.Scheduler.BuildingBlocks.Domain.Guids;
using Mews.Job.Scheduler.Domain.Jobs;
using Mews.Job.Scheduler.Domain.Profiles;
using Mews.Job.Scheduler.Job;

namespace Mews.Job.Scheduler.Domain.JobExecutions;

public sealed class JobExecution : Entity<Guid>, IHaveCreationAndModificationTime, IHaveStartTime
{
    public Guid JobId { get; set; }

    public JobExecutionState State { get; set; }

    public bool IsSuccess => State == JobExecutionState.Success;

    public bool IsTimeout => State == JobExecutionState.Timeout;

    public bool IsProcessed => State != JobExecutionState.InProgress;

    public DateTime StartUtc { get; set; }

    public DateTime? EndUtc { get; set; }

    public string? TransactionIdentifier { get; set; }

    public string? Tag { get; set; }

    public string? ExecutorTypeNameValue { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }

    public DateTime? DeletedUtc { get; set; }

    public Guid CreatorProfileId { get; set; }

    public Guid UpdaterProfileId { get; set; }

    public bool IsDeleted { get; set; }

    public Jobs.Job Job { get; set; } = null!;

    public static JobExecution Create(JobExecutionCreateParameters parameters, DateTime nowUtc, SystemProfile creatorProfile)
    {
        var jobExecution = new JobExecution
        {
            Id = SequentialGuid.Create(),
            JobId = parameters.JobId,
            ExecutorTypeNameValue = parameters.ExecutorTypeNameValue,
            State = parameters.State,
            StartUtc = parameters.StartUtc,
            TransactionIdentifier = parameters.TransactionIdentifier,
            CreatedUtc = nowUtc,
            CreatorProfileId = creatorProfile.Id,
        };
        jobExecution.Updated(nowUtc, creatorProfile);

        return jobExecution;
    }

    public void MarkAsTimedOut(DateTime nowUtc, SystemProfile updaterProfile)
    {
        State = JobExecutionState.Timeout;
        EndUtc = nowUtc;

        Updated(nowUtc, updaterProfile);
    }

    public void UpdateJobExecutionResult(UpdateJobExecutionResultParameters resultParameters, DateTime nowUtc, SystemProfile updaterProfile)
    {
        if (State != JobExecutionState.InProgress)
        {
            throw new StateTransitionException(State.ToString(), resultParameters.State.ToString());
        }

        State = resultParameters.State;
        EndUtc = nowUtc;
        Tag = resultParameters.Tag;

        Updated(nowUtc, updaterProfile);
    }

    private void Updated(DateTime updatedUtc, SystemProfile updaterProfile)
    {
        UpdatedUtc = updatedUtc;
        UpdaterProfileId = updaterProfile.Id;
    }
}
