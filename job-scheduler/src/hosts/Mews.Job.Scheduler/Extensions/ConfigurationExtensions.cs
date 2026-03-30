namespace Mews.Job.Scheduler.Environments;

public static class ConfigurationExtensions
{
    public static void CheckEnvironmentConfiguration(this ConfigurationManager configuration, string environmentValue)
    {
        if (!SupportedEnvironments.IsSupported(environmentValue))
        {
            throw new ArgumentException($"{SupportedEnvironments.UnsupportedEnvironmentErrorMessage}: {environmentValue}");
        }
    }
}
