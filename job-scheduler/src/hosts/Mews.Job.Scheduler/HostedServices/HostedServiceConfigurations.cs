namespace Mews.Job.Scheduler.HostedServices;

public sealed class HostedServiceConfigurations
{
    public const string SectionName = "HostedServiceConfigurations";

    /// <summary>
    /// Collection of configurations, keyed by nameof(TService).
    /// </summary>
    public required IReadOnlyDictionary<string, TimedHostedServiceConfiguration> ConfigurationsByService { get; set;  }
}
