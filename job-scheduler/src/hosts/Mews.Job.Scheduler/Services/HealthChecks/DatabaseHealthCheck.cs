using Medallion.Threading.SqlServer;
using Mews.Job.Scheduler.Core.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Mews.Job.Scheduler.Services.HealthChecks;

internal class DatabaseHealthCheck : IHealthCheck
{
    private readonly ILogger<DatabaseHealthCheck> _logger;
    private readonly IConfiguration _configuration;
    private readonly JobSchedulerDbContext _jobSchedulerDbContext;

    public DatabaseHealthCheck(
        IConfiguration configuration,
        JobSchedulerDbContext jobSchedulerDbContext,
        ILogger<DatabaseHealthCheck> logger)
    {
        _logger = logger;
        _configuration = configuration;
        _jobSchedulerDbContext = jobSchedulerDbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var dbConnectionString = _configuration.GetConnectionString(ApiModuleConstants.ConnectionStringName);
        
        if(string.IsNullOrWhiteSpace(dbConnectionString))
        {
            _logger.LogError("Connection string is not set");
            return HealthCheckResult.Unhealthy("Connection string is not set");
        }
        
        var @lock = new SqlDistributedLock("DBMigrationLock", dbConnectionString);

        try
        {
            await using (await @lock.AcquireAsync(TimeSpan.FromSeconds(3), cancellationToken: cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Cancellation was requested before starting the migration process");
                    return HealthCheckResult.Unhealthy("Migration cancelled before start");
                }

                await _jobSchedulerDbContext.Database.MigrateAsync(cancellationToken);
                
                return HealthCheckResult.Healthy();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DB migration operation was cancelled");
            return HealthCheckResult.Unhealthy("DB migration was cancelled");
        }
        catch (Exception ex)
        {
            var errorId = Guid.NewGuid();
            _logger.LogError(ex, "DB migration encountered an error ({errorId})", errorId);
            return HealthCheckResult.Unhealthy($"DB migration has error ({errorId})");
        }
    }
}
