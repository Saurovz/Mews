namespace Mews.Job.Scheduler.Aspire.AppHost;

public static class AspireAppHostConfiguration
{
    public const string ProjectResourceName = "mewsJobScheduler";
    public const string MigrationProjectResourceName = "mewsJobSchedulerMigration";
    public const string SqlServerResourceName = "job-scheduler-mssql-container-aspire";
    public const int SqlContainerPort = 14347;
    public const string SqlServerDatabaseName = "JobSchedulerDatabase";
    public const string TemporalServerResourceName = "temporalServer";
}
