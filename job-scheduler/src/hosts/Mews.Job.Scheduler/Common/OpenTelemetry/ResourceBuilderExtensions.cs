using OpenTelemetry.Resources;

namespace Mews.Job.Scheduler.Common.OpenTelemetry;

public static class ResourceBuilderExtensions
{
    /// <summary>
    /// Adds any values it finds from the Azure Container Apps environment variables.
    /// <see href="https://learn.microsoft.com/en-us/azure/container-apps/environment-variables">Azure Container Apps Environment Variables</see>
    /// </summary>
    /// <param name="resourceBuilder"></param>
    /// <returns></returns>
    public static ResourceBuilder AddAzureContainerAppAttributes(this ResourceBuilder resourceBuilder)
    {
        var envVars = Environment.GetEnvironmentVariables();

        // List of Azure Container Apps environment variables to add as attributes
        var envVarsToAdd = new List<Tuple<string, string>>
        {
            new("service.instance.id", "CONTAINER_APP_REVISION"),
            new("azure.containerapp.revision", "CONTAINER_APP_REVISION"),
            new("azure.containerapp.replica_name", "CONTAINER_APP_REPLICA_NAME")
        };

        // Add attributes to resource builder based on environment variables found
        resourceBuilder.AddAttributes(
            envVarsToAdd
                .Where(attr => envVars.Contains(attr.Item2) &&
                               !string.IsNullOrEmpty(envVars[attr.Item2]?.ToString()))
                .Select(attr =>
                {
                    var (name, key) = attr;
                    return new KeyValuePair<string, object>(name, envVars[key]!.ToString()!);
                })
        );

        return resourceBuilder;
    }
}
