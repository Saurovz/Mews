using System.Reflection;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;
using LaunchDarkly.Sdk.Server;
using LaunchDarkly.Sdk.Server.Interfaces;
using Mews.Atlas.Alerting;
using Mews.Atlas.AspNetCore.Security;
using Mews.Atlas.FeatureFlags;
using Mews.Job.Scheduler.BuildingBlocks.Configuration.Alerting;
using Mews.Job.Scheduler.BuildingBlocks.Configuration.Security;
using Mews.Job.Scheduler.Configuration;
using Mews.Job.Scheduler.Configuration.Secrets;
using Mews.Job.Scheduler.Environments;
using Mews.Job.Scheduler.Extensions;

namespace Mews.Job.Scheduler.Services;

public static class AtlasServiceConfiguration
{
    public static IServiceCollection AddKeyVault(this IServiceCollection services, ConfigurationManager configuration, string environment)
    {
        var keyVaultUri = configuration.GetValue<string>(KeyVaultConfiguration.ValueFieldName) ??
                          services.TryAddConfiguration<KeyVaultConfiguration>(configuration, KeyVaultConfiguration.SectionName)?.Uri;
        var isKeyVaultUriEmpty = string.IsNullOrWhiteSpace(keyVaultUri);
        var isLocal = SupportedEnvironments.IsLocalEnvironment(environment);
        var isAspire = SupportedEnvironments.IsAspireEnvironment(environment);

        if (isLocal || isAspire)
        {
            configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
        }

        if (!isKeyVaultUriEmpty)
        {
            // Get token credentials using the Mews Atlas workflow
            var tokenCredential = new Mews.Atlas.Azure.Identity.TokenCredentialFactory().Create(isLocal);
            var client = new SecretClient(new Uri(keyVaultUri!), tokenCredential);
            services.AddSingleton(client);
            configuration.AddAzureKeyVault(client, new AzureKeyVaultConfigurationOptions());
        }

        return services;
    }

    public static IServiceCollection AddFeatureFlags(this IServiceCollection services, IConfiguration configuration, string environment)
    {
        var launchDarklyConfiguration = services.TryAddConfiguration<LaunchDarklyConfiguration>(configuration, LaunchDarklyConfiguration.SectionName);
        var missingSdkKey = launchDarklyConfiguration is null || string.IsNullOrWhiteSpace(launchDarklyConfiguration.SdkKey);

        if (missingSdkKey || SupportedEnvironments.IsLocalEnvironment(environment))
        {
            var featureFlagConfiguration = services.TryAddConfiguration<FeatureFlagConfiguration>(configuration, FeatureFlagConfiguration.SectionName);
            var featureFlagOptions = new FeatureFlagOptions(featureFlagConfiguration?.FeatureFlags ?? new List<FeatureFlag>());

            services.AddSingleton<IFeatureFlagOptions>(featureFlagOptions);
            services.AddSingleton<IFeatureFlagService, LocalFeatureFlagService>();
        }
        else
        {
            services.AddSingleton<ILdClient>(_ => new LdClient(launchDarklyConfiguration!.SdkKey));
            services.AddSingleton<IFeatureFlagService, LaunchDarklyFeatureFlagService>();
        }

        return services;
    }


    public static IServiceCollection AddSecurityHeaders(this IServiceCollection services)
    {
        services.AddTransient<SecurityHeadersMiddleware>();
        services.Configure<SecurityHeadersOptions>(options =>
        {
            foreach (var (header, value) in SecurityHeadersConfiguration.HeaderValues)
            {
                options.Headers.Add(header, value);
            }
        });

        return services;
    }

    public static IServiceCollection AddIncidentReporter(this IServiceCollection services, IConfiguration configuration, string environment)
    {
        var sentryConfigurations = services.TryAddConfiguration<SentryConfigurations>(configuration, SentryConfigurations.SectionName);

        if (SupportedEnvironments.IsLocalEnvironment(environment) || sentryConfigurations is null)
        {
            services.AddSingleton<IIncidentReporter, LocalIncidentReporter>();
        }
        else
        {
            var sentryOptions = CreateOptions(sentryConfigurations.Default);
            services.AddSingleton<ISentryOptions>(sentryOptions);
            services.AddSingleton<IIncidentReporter, SentryIncidentReporter>();

            services.AddTeamIncidentReporters(sentryConfigurations.TeamConfigurations);
        }

        return services;
    }

    private static IServiceCollection AddTeamIncidentReporters(
        this IServiceCollection services,
        IReadOnlyDictionary<string, SentryOptionsConfiguration> configurationsByTeam)
    {
        foreach (var (team, configuration) in configurationsByTeam)
        {
            // DSN is defined elsewhere and environment is defined in the appsettings JSON files,
            // when loading configurations it can happen that either will be null if it is missing.
            // We need to add actual validation for this issue but for now it makes it easier to introduce the teams en-mass.
            if (configuration.Dsn is null || configuration.Environment is null)
            {
                continue;
            }

            var sentryOptions = CreateOptions(configuration);
            services.AddKeyedSingleton<IIncidentReporter, SentryIncidentReporter>(
                serviceKey: team,
                implementationFactory: (sp, _) => new SentryIncidentReporter(
                    logger: sp.GetRequiredService<ILogger<SentryIncidentReporter>>(),
                    configuration: sentryOptions
                )
            );
        }

        return services;
    }

    private static Atlas.Alerting.SentryOptions CreateOptions(SentryOptionsConfiguration configuration)
    {
        var release = configuration.Release ?? Assembly.GetExecutingAssembly().GetName().Version!.ToString();

        return new Atlas.Alerting.SentryOptions(
            dsn: configuration.Dsn,
            environment: configuration.Environment,
            release: release
        );
    }
}
