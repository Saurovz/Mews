namespace TaxManager.Aspire.AppHost;

public static class AspireAppHostConfiguration
{
    public const string ProjectResourceName = "taxmanager-api";
    public const string MigrationProjectResourceName = "taxmanager-migration";
    public const string WebProjectResourceName = "frontend";
    public const string SqlServerResourceName = "sql-server";
    public const int SqlContainerPort = 14353;
    public const string RedisResourceName = "redis-server";
    public const int RedisContainerPort = 63132;   
    public const int RedisCommanderContainerPort = 8081;
    public const string SqlServerDatabaseName = "taxmanager-local-db";
}
