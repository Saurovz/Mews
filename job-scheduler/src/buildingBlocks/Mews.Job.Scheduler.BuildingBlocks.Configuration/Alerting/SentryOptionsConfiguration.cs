namespace Mews.Job.Scheduler.BuildingBlocks.Configuration.Alerting;

public sealed class SentryOptionsConfiguration
{
    public required string Dsn { get; set; }

    public required string Environment { get; set; }

    public string? Release { get; set; }
}
