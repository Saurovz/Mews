using Mews.Job.Scheduler.BuildingBlocks.Domain;
using Mews.Job.Scheduler.BuildingBlocks.Domain.Guids;
using Mews.Job.Scheduler.Domain.JobExecutions;
using Mews.Job.Scheduler.BuildingBlocks.Types;
using Mews.Job.Scheduler.BuildingBlocks.Types.DateTimeSpan;
using Mews.Job.Scheduler.Domain.Executors;
using Mews.Job.Scheduler.Domain.Profiles;
using Mews.Job.Scheduler.Job;

namespace Mews.Job.Scheduler.Domain.Jobs;

public sealed class Job : Entity<Guid>, IHaveCreationAndModificationTime, IHaveStartTime
{
    public const int TimeoutRetryAttemptLimit = 5;
    public const int BackoffInMilliseconds = 1000;

    private DateTimeSpan? _period;
    private DateTimeSpan? _maxExecutionTime;

    public Job()
    {
        JobExecutions = new List<JobExecution>();
    }

    public byte[] EntityVersion { get; set; } = null!;

    public string? NameNew { get; set; }

    public Guid ExecutorId { get; set; }

    public Executor Executor { get; set; }

    public string FullName => string.Join("/", Executor.Type, NameNew);

    public string? Data { get; set; }

    public JobLogVerbosity LogVerbosity { get; set; }

    public JobState State { get; set; }

    public string? PeriodValue { get; private set; }

    public DateTimeSpan? Period
    {
        get => _period ??= PeriodValue.ToNullableDateTimeSpan();
        set
        {
            _period = value.HasValue && value.Value.InRange(from: DateTimeSpan.Zero, closed: false)
                ? value
                : null;
            PeriodValue = value?.ToString();
        }
    }

    public bool IsPeriodical => Period.HasValue;

    public DateTime StartUtc { get; set; }

    public DateTime? ScheduledUtc { get; set; }

    public DateTime? ExecutionStartUtc { get; set; }

    public DateTime? PreviousSuccessfulStartUtc { get; set; }

    public string MaxExecutionTimeValue { get; private set; }

    public DateTimeSpan MaxExecutionTime
    {
        get => _maxExecutionTime ??= MaxExecutionTimeValue.ToDateTimeSpan()!.Value;
        set
        {
            _maxExecutionTime = value;
            MaxExecutionTimeValue = value.ToString();
        }
    }

    public JobOptions Options { get; set; }

    public bool TimeoutRetryDisabled => (Options & JobOptions.TimeoutRetryDisabled) == JobOptions.TimeoutRetryDisabled;

    public bool TimeoutAsWarning => (Options & JobOptions.TimeoutAsWarning) == JobOptions.TimeoutAsWarning;

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }

    public DateTime? DeletedUtc { get; set; }

    public Guid CreatorProfileId { get; set; }

    public Guid UpdaterProfileId { get; set; }

    public bool IsDeleted { get; set; }

    public bool IsTimedOut(DateTime nowUtc) =>
        State == JobState.InProgress &&
        ExecutionStartUtc.HasValue &&
        ExecutionStartUtc.Value.Add(MaxExecutionTime) < nowUtc;

    public ICollection<JobExecution> JobExecutions { get; }

    public static Job Create(JobCreateParameters parameters, Executor executor, DateTime nowUtc)
    {
        var job = new Job
        {
            Id = SequentialGuid.Create(),
            NameNew = parameters.Name,
            ExecutorId = executor.Id,
            Executor = executor,
            Data = parameters.Data,
            State = JobState.Pending,
            Period = parameters.Period,
            StartUtc = parameters.StartUtc,
            MaxExecutionTime = parameters.MaxExecutionTime,
            Options = parameters.Options,
            CreatedUtc = nowUtc,
            CreatorProfileId = parameters.CreatorProfileId
        };
        job.Updated(nowUtc, parameters.CreatorProfileId);

        return job;
    }

    public void Update(JobUpdateParameters parameters, Executor executor, DateTime nowUtc)
    {
        NameNew = parameters.Name;
        ExecutorId = executor.Id;
        Executor = executor;
        StartUtc = parameters.StartUtc;
        Period = parameters.Period;
        MaxExecutionTime = parameters.MaxExecutionTime;
        Options = parameters.Options;
        Data = parameters.Data;
        Updated(nowUtc, parameters.UpdaterProfileId);
    }

    public void MarkAsTimedOut(DateTime nowUtc, int timeoutRetryCount, SystemProfile updaterProfile)
    {
        if (IsPeriodical)
        {
            State = JobState.Pending;

            StartUtc = GetNextStartUtc(Period!.Value, nowUtc, timeoutRetryCount, lastExecutionTimedOut: true);

            Updated(updatedUtc: nowUtc, updaterProfileId: updaterProfile.Id);
        }
        else
        {
            State = JobState.Executed;

            SoftDelete(deletedUtc: nowUtc, updaterProfileId: updaterProfile.Id);
        }
    }

    public void TryMarkAsScheduled(DateTime nowUtc, SystemProfile updaterProfile)
    {
        if (State is JobState.Pending or JobState.Scheduled)
        {
            State = JobState.Scheduled;
            ScheduledUtc = nowUtc;

            Updated(updatedUtc: nowUtc, updaterProfileId: updaterProfile.Id);

            return;
        }

        throw new StateTransitionException(State.ToString(), JobState.Scheduled.ToString());
    }

    public bool TryMarkAsInProgress(DateTime nowUtc, SystemProfile updaterProfile)
    {
        if (State == JobState.Scheduled)
        {
            State = JobState.InProgress;
            ExecutionStartUtc = nowUtc;

            Updated(updatedUtc: nowUtc, updaterProfileId: updaterProfile.Id);

            return true;
        }

        if (State == JobState.InProgress)
        {
            return false;
        }

        throw new EntityProcessingException(Id, FullName);
    }

    public void ConfirmAfterExecution(ConfirmJobAfterExecutionParameters parameters, DateTime nowUtc, SystemProfile updaterProfile)
    {
        var state = IsPeriodical ?
            parameters.DeleteJob ? JobState.Executed : JobState.Pending :
            JobState.Executed;

        if (State != JobState.InProgress)
        {
            throw new StateTransitionException(State.ToString(), state.ToString());
        }

        if (IsPeriodical)
        {
            if (parameters.IsExecutionSuccess)
            {
                PreviousSuccessfulStartUtc = StartUtc;
            }
            if (parameters.FutureRunData != null)
            {
                Data = parameters.FutureRunData;
            }

            StartUtc = GetNextStartUtc(Period!.Value, nowUtc, parameters.TimeoutRetryCount, parameters.IsExecutionTimedOut);
            ExecutionStartUtc = null;
            State = state;
        }
        else
        {
            State = state;
        }

        Updated(nowUtc, updaterProfile.Id);
    }

    public void SoftDelete(DateTime deletedUtc, Guid updaterProfileId)
    {
        IsDeleted = true;
        DeletedUtc = deletedUtc;

        Updated(updatedUtc: deletedUtc, updaterProfileId: updaterProfileId);
    }

    private DateTime GetNextStartUtc(DateTimeSpan period, DateTime nowUtc, int timeoutRetryCount, bool lastExecutionTimedOut)
    {
        var timeoutRetryDisabled = TimeoutRetryDisabled || TimeoutRetryAttemptLimit == 0;
        var nextPeriodicStartUtc = GetNextPeriodicalStartUtc(period, nowUtc);
        if (!lastExecutionTimedOut || timeoutRetryDisabled)
        {
            return nextPeriodicStartUtc;
        }
        var nextBackedOffStartUtc = GetNextBackedOffStartUtc(nowUtc, timeoutRetryCount);

        return nextPeriodicStartUtc < nextBackedOffStartUtc
            ? nextPeriodicStartUtc
            : nextBackedOffStartUtc;
    }

    private DateTime GetNextPeriodicalStartUtc(DateTimeSpan period, DateTime nowUtc)
    {
        var periodTicks = period.ToTimeSpan().Ticks;
        var elapsed = nowUtc - StartUtc;
        var elapsedPeriodCount = elapsed.Ticks / periodTicks;

        return StartUtc.Add(TimeSpan.FromTicks(periodTicks * (elapsedPeriodCount + 1)));
    }

    private DateTime GetNextBackedOffStartUtc(DateTime nowUtc, int timeoutRetryCount)
    {
        return nowUtc.AddMilliseconds(BackoffInMilliseconds * (int)Math.Pow(2, timeoutRetryCount));
    }

    private void Updated(DateTime updatedUtc, Guid updaterProfileId)
    {
        UpdatedUtc = updatedUtc;
        UpdaterProfileId = updaterProfileId;
    }
}
