using Serilog;

namespace Mews.Job.Scheduler.Extensions;

public static class ServiceCollectionExtensions
{
    public static TConfiguration AddConfiguration<TConfiguration>(this IServiceCollection services, IConfiguration configuration, string key)
        where TConfiguration : class
    {
        var specificConfiguration = services.TryAddConfiguration<TConfiguration>(configuration, key);

        if (specificConfiguration is not null)
        {
            return specificConfiguration;
        }

        var missingConfigurationErrorMessage = $"Please provide {key} configuration with all its properties.";
        Log.Error(missingConfigurationErrorMessage);

        throw new ArgumentException(missingConfigurationErrorMessage);
    }

    public static TConfiguration? TryAddConfiguration<TConfiguration>(this IServiceCollection services, IConfiguration configuration, string key)
        where TConfiguration : class
    {
        var configurationSection = configuration.GetSection(key);
        var specificConfiguration = configurationSection.Get<TConfiguration>();

        if (specificConfiguration is null)
        {
            return null;
        }

        services.Configure<TConfiguration>(configurationSection);

        return specificConfiguration;
    }
}
