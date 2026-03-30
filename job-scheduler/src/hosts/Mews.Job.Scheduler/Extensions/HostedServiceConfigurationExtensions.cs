using Mews.Job.Scheduler.HostedServices;

namespace Mews.Job.Scheduler.Extensions;

internal static class HostedServiceConfigurationExtensions
{
    public static T GetValueFromAdditionalParameters<T>(this TimedHostedServiceConfiguration configuration, string key)
    {
        var additionalParameters = configuration.AdditionalParameters ?? throw new InvalidOperationException($"{nameof(configuration.AdditionalParameters)} are not set.");

        var valueByKey = additionalParameters[key];
        var result = valueByKey != null 
            ? (T)Convert.ChangeType(valueByKey, typeof(T))
            : throw new InvalidOperationException($"The configuration parameter '{key}' is missing.");

        return result;
    }
}
