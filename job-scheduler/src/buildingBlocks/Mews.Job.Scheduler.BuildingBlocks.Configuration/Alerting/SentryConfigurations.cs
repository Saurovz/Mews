namespace Mews.Job.Scheduler.BuildingBlocks.Configuration.Alerting;

public sealed class SentryConfigurations
{
    public const string SectionName = "SentryConfigurations";

    public required SentryOptionsConfiguration Default { get; set; }

    public required IReadOnlyDictionary<string, SentryOptionsConfiguration> TeamConfigurations { get; set; }
}
