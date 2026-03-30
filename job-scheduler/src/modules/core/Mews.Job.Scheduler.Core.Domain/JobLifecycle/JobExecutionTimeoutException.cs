using Mews.Atlas.Alerting;
using Mews.Job.Scheduler.BuildingBlocks.Domain.PlatformTeams;
using Mews.Job.Scheduler.Domain.JobExecutions;

namespace Mews.Job.Scheduler;

public sealed class JobExecutionTimeoutException : Exception
{
    private JobExecutionTimeoutException(
        string message,
        string team,
        IncidentLevel level,
        object details)
        : base(message)
    {
        Team = team;
        IncidentLevel = level;
        Details = details;
    }

    public string Team { get; }

    public IncidentLevel IncidentLevel { get; }

    public object Details { get; }

    public static JobExecutionTimeoutException Create(JobExecution execution)
    {
        var team = execution.Job.Executor.Team;
        var isWarning = execution.Job.IsPeriodical || execution.Job.TimeoutAsWarning;
        var incidentLevel = isWarning
            ? IncidentLevel.Warning
            : IncidentLevel.Error;
        var details = new
        {
            Data = execution.Job.Data,
            Execution = new
            {
                StartUtc = execution.StartUtc,
                Id = execution.Id,
                TransactionId = execution.TransactionIdentifier
            }
        };

        return new JobExecutionTimeoutException(
            message: $"{execution.Job.FullName} timeout.",
            team: team,
            level: incidentLevel,
            details: details
        );
    }
}
