namespace TaxManager.Configuration;

/// <summary>
/// Provides constants that define the names of various environments.
/// These environment names are used for conditional logic based on the runtime environment.
/// Developers should adapt these constants according to the specific needs and naming conventions of their projects.
/// Note: Developers are encouraged to update this class with environment names that are valid for their specific service.
/// </summary>
public static class SupportedEnvironments
{
    /// <summary>
    /// Represents the local development environment.
    /// This constant is used to apply settings or behaviors that should only occur when running locally for development.
    /// </summary>
    public const string LocalDevelopment = "localDev";


    private static readonly HashSet<string> All = new()
    {
        LocalDevelopment
    };

    public static bool IsSupported(string environment) => All.Contains(environment);

    public static bool IsLocalDevelopment(this string environmentName) => environmentName == LocalDevelopment;
}
