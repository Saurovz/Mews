namespace Mews.Job.Scheduler.Configuration;

public sealed class SqlConfiguration
{
    public const string SectionName = "SqlConfiguration";

    public required string SchemaName { get; set; }

    public required string ConnectionString { get; set; }
    
    public required RetryExecutionStrategy RetryExecutionStrategy { get; set; }
}

public sealed record RetryExecutionStrategy(int MaxRetryCount, TimeSpan MaxRetryDelay)
{
    public static RetryExecutionStrategy Default => new RetryExecutionStrategy(3, TimeSpan.FromSeconds(20));
}
