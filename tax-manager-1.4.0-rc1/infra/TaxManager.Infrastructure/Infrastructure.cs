using Mews.Infrastructure.Sdk.ContainerApp;
using Mews.Infrastructure.Sdk.ResourceDeployment;

namespace TaxManager.Infrastructure;

public static class Infrastructure
{
    public static Dictionary<string, object> Create()
    {
        var builder = new InfrastructureBuilder();
        var serviceName = builder.GetString("service-name");
        var environment = builder.GetString("service-environment");

        foreach (var location in builder.Locations)
        {
            // Add your services here.
            // See the documentation at https://mews.atlassian.net/wiki/spaces/AP/pages/40337422/SDK.
            var app = builder.AddContainerApp(new ContainerAppArgsV2(
                Location: location,
                ImageName: Environment.GetEnvironmentVariable("TARGET_IMAGE_NAME")
                           ?? throw new Exception("Mandatory environment variable TARGET_IMAGE_NAME is not provided."),
                IngressConfiguration: new ContainerAppIngressAllowed(true),
                ContainerAppSettings: new ContainerAppSettings(),
                ContainerName: serviceName,
                EnvironmentVersion: EnvironmentType.WorkloadProfile,
                Secrets: new []
                {
                    new SecretEnvironmentVariable(
                        secretName:"some-secret-value",
                        variableName: "SomeSecret",
                        value: builder.GetSecret("some-secret-value"))
                },
                EnvironmentVariables: [
                    ..OtelHelper.GetOtelConfigEnvironmentVariables(serviceName, environment)
                ]
            ));
        }
        
        return builder.Build();
    }
}
