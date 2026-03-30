using Mews.Atlas.FeatureFlags;

namespace Mews.Job.Scheduler.Configuration;

public sealed class FeatureFlagConfiguration
{
    public const string SectionName = "FeatureFlagConfiguration";

    public IEnumerable<FeatureFlag>? FeatureFlags { get; set; }
}
