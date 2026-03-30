namespace Mews.Job.Scheduler.Environments;

public static class SupportedEnvironments
{
    public const string UnsupportedEnvironmentErrorMessage = "Unsupported environment";
    public const string LocalDevelopmentEnvironment = "localDev";
    public const string DockerizedLocalDevelopmentEnvironment = "localDocker";
    public const string DevelopmentEnvironment = "dev";
    public const string TestEnvironment = "test";
    public const string DemoEnvironment = "demo";
    public const string ProductionEnvironment = "prod";
    public const string AspireEnvironment = "localAspire";

    private static readonly HashSet<string> All = new()
    {
        LocalDevelopmentEnvironment,
        DockerizedLocalDevelopmentEnvironment,
        DevelopmentEnvironment,
        TestEnvironment,
        DemoEnvironment,
        ProductionEnvironment,
        AspireEnvironment
    };

    public static bool IsSupported(string environment)
    {
        return All.Contains(environment);
    }

    public static bool IsLiveEnvironment(string environment)
    {
        return environment is DevelopmentEnvironment or TestEnvironment or DemoEnvironment or ProductionEnvironment;
    }

    public static bool IsDeveloperEnvironment(string environment)
    {
        return environment is DevelopmentEnvironment or TestEnvironment or LocalDevelopmentEnvironment or DockerizedLocalDevelopmentEnvironment;
    }

    public static bool IsLocalEnvironment(string environment)
    {
        return environment is LocalDevelopmentEnvironment or DockerizedLocalDevelopmentEnvironment;
    }

    public static bool IsAspireEnvironment(string environment)
    {
        return environment is AspireEnvironment;
    }
}
