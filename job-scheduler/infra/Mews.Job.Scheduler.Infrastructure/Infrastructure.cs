using Mews.Infrastructure.Resources;
using Mews.Infrastructure.Resources.ServiceBus;
using Mews.Infrastructure.Sdk.ContainerApp;
using Mews.Infrastructure.Sdk.KeyVault;
using Mews.Infrastructure.Sdk.Primitives;
using Mews.Infrastructure.Sdk.ResourceDeployment;
using Mews.Infrastructure.Sdk.ServiceBus;
using Mews.Infrastructure.Sdk.ServiceBus.Access;
using Mews.Infrastructure.Sdk.ServiceBus.Subscription;
using Mews.Infrastructure.Sdk.ServiceBus.Topic;
using Mews.Infrastructure.Sdk.Sql;
using Pulumi;
using Pulumi.AzureNative.KeyVault;

namespace Mews.Job.Scheduler.Infrastructure;

public static class Infrastructure
{
    public static Dictionary<string, object> Create()
    {
        var builder = new InfrastructureBuilder();

        var activeDirectoryPrincipals = new[]
        {
            AzureActiveDirectory.GetGroup("eng.pe.dx.tooling")
        };

        var vaultAccessPermissions = new[]
        {
            SecretPermissions.Get,
            SecretPermissions.List,
            SecretPermissions.Set,
            SecretPermissions.Delete,
            SecretPermissions.Purge
        };

        builder.AddResourceGroupReaders(activeDirectoryPrincipals);

        var serviceName = builder.GetString("service-name");
        var environment = builder.GetString("service-environment");
        var serviceBusLocations = builder.ServiceBusLocations;
        var westAndNorthEuropeAzureLocation =
            new AzureLocations(AzureLocation.WestEurope, AzureLocation.NorthEurope).ToArray();

        var otelVars = OtelHelper.GetOtelConfigEnvironmentVariables(serviceName, environment).Select(a =>
            a.Name == "OTEL_SERVICE_NAME"
                ? new PlainEnvironmentVariable("OTEL_SERVICE_NAME",
                    a.Value.Apply(b => b + "-api")) // compatability, we use the -api suffix
                : a).ToArray();


        var applicationSupportedServiceBusLocations = environment switch
        {
            "dev" => westAndNorthEuropeAzureLocation,
            "demo" => westAndNorthEuropeAzureLocation,
            "prod" => westAndNorthEuropeAzureLocation,
            "test" => new AzureLocations(AzureLocation.GermanyWestCentral).ToArray(),
            _ => throw new Exception($"Unsupported environment: {environment}")
        };

        AddJobSchedulingRequestSb(builder, [..serviceBusLocations]);
        AddJobSchedulingRequestLocalDevelopmentSb(builder, [..serviceBusLocations]);
        
        var appRegistration = builder.AddS2SApplicationRegistration();

        foreach (var location in builder.Locations)
        {
            // Add your services here.
            // See the documentation at https://mews.atlassian.net/wiki/spaces/AP/pages/40337422/SDK.

            var keyVault = builder.AddKeyVault(new KeyVaultArgs(
              FullName:$"{serviceName}-{environment}-main",
                Location: location,
                Name: "main",
                AccessPolicies: activeDirectoryPrincipals.Select(principal => new KeyVaultAccessPolicy(
                    Principal: principal,
                    SecretPermissions: vaultAccessPermissions,
                    CertificatePermissions: [],
                    KeyPermissions: []
                ))
            ));
            var app = builder.AddContainerApp(new ContainerAppArgsV2(
                Location: location,
                ImageName: Environment.GetEnvironmentVariable("TARGET_IMAGE_NAME")
                           ?? throw new Exception("Mandatory environment variable TARGET_IMAGE_NAME is not provided."),
                IngressConfiguration: ContainerAppIngressConfiguration.Allowed(
                    ExternalIngressAllowed: true,
                    CustomDomain: builder.GetOptionalString("custom-domain")
                ),
                ContainerAppSettings: null,
                ContainerName: serviceName,
                Name: "api",
                DeployToPairedRegion: builder.GetBool("container-app-deploy-to-paired-region"),
                Secrets: [],
                EnvironmentVersion: EnvironmentType.WorkloadProfile,
                EnvironmentVariables:
                [
                    ..otelVars,
                    new PlainEnvironmentVariable("OTEL_METRIC_EXPORT_INTERVAL", "5000"),
                ]
            ));

            var databaseConfiguration = new SqlDatabaseConfiguration(builder);
            var database = builder.AddSqlDatabase(
                new SqlDatabaseArgs(
                    Server: new SqlServerArgs(
                        Location: location,
                        AdminLogin: builder.GetString("sql-server-admin-login"),
                        AdminPassword: new KeyVaultSecretValueSource(keyVault, "sql-admin-password"),
                        PrivateEndpoint: true
                    ),
                    Name: $"{serviceName}-{environment}",
                    Tier: databaseConfiguration.Tier,
                    ZoneRedundancy: databaseConfiguration.ZoneRedundancy,
                    HighAvailabilityReplicaCount: databaseConfiguration.HighAvailabilityReplicaCount,
                    BackupStorageRedundancy: databaseConfiguration.BackupStorageRedundancy,
                    GeoReplicaInPairedRegion: databaseConfiguration.GeoReplicaInPairedRegion
                )
            );
            
            app.InjectApplicationRegistrationInfo(appRegistration);
            app.InjectSqlConnectionStrings(database, "JobSchedulerDatabase");
            app.InjectServiceBusNamespace(
                instance: ServiceBusInstance.SharedPremium,
                namespaceEnvironmentVariableName:
                "Messaging__ServiceBus__Connections__Scheduler__FullyQualifiedNamespace",
                locations: [..applicationSupportedServiceBusLocations]
            );
            app.InjectServiceBusEndpoint(
                instance: ServiceBusInstance.SharedPremium,
                endpointEnvironmentVariableName: "SERVICE_BUS_URL",
                locations: [..applicationSupportedServiceBusLocations]
            );
            app.InjectKeyVaultURI(keyVault);
        }

        return builder.Build();
    }

    private static void AddJobSchedulingRequestSb(InfrastructureBuilder builder, AzureLocations locations)
    {
        const string serviceBusTopicName = "job-scheduler-notifications";
        builder.AddServiceBusTopic(new ServiceBusTopicArgs(
            Name: serviceBusTopicName,
            AccessType: ServiceBusAccessType.Send,
            Instance: ServiceBusInstance.SharedPremium,
            Locations: locations,
            // Needs to be similar with message TTL, so that in case of expiration the message is re-queued immediately.
            DuplicateDetection: DuplicateDetection.Enabled(TimeSpan.FromMinutes(3))
        ));
        builder.AddServiceBusSubscription(new ServiceBusSubscriptionArgs(
            Name: "job-scheduler-notifications-monolith",
            Locations: locations,
            TopicName: serviceBusTopicName,
            AccessType: ServiceBusAccessType.Receive,
            Instance: ServiceBusInstance.SharedPremium,
            LockDuration: TimeSpan.FromSeconds(60),
            MaxDeliveryCount: 3,
            // Needs to be reasonably above the secondary job instance execution delay which is 2 minutes in develop, 1 minute elsewhere
            // See https://github.com/MewsSystems/mews/blob/develop/src/Framework/Server/Mews.Server.Configurations/Instances/InstanceJobConfiguration.cs#L15
            DefaultMessageTimeToLive: TimeSpan.FromMinutes(3),
            DeadLetteringOnMessageExpiration: true
        ));
    }

    private static void AddJobSchedulingRequestLocalDevelopmentSb(InfrastructureBuilder builder,
        AzureLocations locations)
    {
        if (!(builder.GetOptionalBool("deploy-local-development-sb") ?? false))
        {
            return;
        }

        const string serviceBusTopicName = "job-scheduler-local-development";
        builder.AddServiceBusTopic(new ServiceBusTopicArgs(
            Name: serviceBusTopicName,
            AccessType: ServiceBusAccessType.Send,
            Instance: ServiceBusInstance.SharedPremium,
            Locations: locations,
            // Needs to be similar with message TTL, so that in case of expiration the message is re-queued immediately.
            DuplicateDetection: DuplicateDetection.Enabled(TimeSpan.FromMinutes(3))
        ));
        builder.AddServiceBusSubscription(new ServiceBusSubscriptionArgs(
            Name: "job-scheduler-monolith-local-development",
            Locations: locations,
            TopicName: serviceBusTopicName,
            AccessType: ServiceBusAccessType.Receive,
            Instance: ServiceBusInstance.SharedPremium,
            LockDuration: TimeSpan.FromSeconds(60),
            MaxDeliveryCount: 3,
            DefaultMessageTimeToLive: TimeSpan.FromMinutes(3),
            AutoDeleteOnIdleTimeout: TimeSpan.FromDays(14),
            DeadLetteringOnMessageExpiration: true
        ));
    }
}
