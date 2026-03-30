namespace Mews.Job.Scheduler.Aspire.IntegrationTests.Configuration;

public static class TestConfiguration
{
    public const string SqlServerContainerName = "mssql-job-scheduler-mssql-container";
    public const string DockerComposeSqlServerFilePath = "docker-compose-sql-server.yml";
    public const string LocalAuthorizationHeaderValue = "token";
}
