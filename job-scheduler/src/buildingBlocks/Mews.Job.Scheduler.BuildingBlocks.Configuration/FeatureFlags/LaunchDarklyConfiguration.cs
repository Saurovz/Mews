namespace Mews.Job.Scheduler.Configuration;

public sealed class LaunchDarklyConfiguration
{
    public const string SectionName = "LaunchDarklyConfiguration";

    public required string SdkKey { get; set; }
}
