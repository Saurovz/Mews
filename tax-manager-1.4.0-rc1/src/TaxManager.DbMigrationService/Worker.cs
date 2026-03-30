using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using TaxManager.EntityFrameworkCore.Data;

namespace TaxManager.DbMigrationService;

public class Worker : BackgroundService
{
    private readonly IServiceProvider serviceProvider;
    private readonly IHostApplicationLifetime hostApplicationLifetime;
    private readonly ILogger<Worker> _logger;
   
    internal const string ActivityName = "MigrationService";
    private static readonly ActivitySource _activitySource = new(ActivityName);

    public Worker(IServiceProvider serviceProvider,
        IHostApplicationLifetime hostApplicationLifetime,
        ILogger<Worker> logger)
    {
        this.serviceProvider = serviceProvider;
        this.hostApplicationLifetime = hostApplicationLifetime;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = _activitySource.StartActivity("Migrating database", ActivityKind.Client);
            
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        }
        
        await EnsureDatabaseAsync(dbContext, stoppingToken);
        await RunMigrationAsync(dbContext, stoppingToken);
        await SeedDataAsync(dbContext, stoppingToken);

        hostApplicationLifetime.StopApplication();
    }
    
    private static async Task EnsureDatabaseAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var dbCreator = dbContext.GetService<IRelationalDatabaseCreator>();

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Create database if it does not exist.
            if (!await dbCreator.ExistsAsync(cancellationToken))
            {
                await dbCreator.CreateAsync(cancellationToken);
            }
        });
    }
    
    private static async Task RunMigrationAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Run migration in a transaction to avoid partial migration if it fails.
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            await dbContext.Database.MigrateAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }
    
    private static Task SeedDataAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        DataSeeder.SeedData(dbContext);
        
        return Task.CompletedTask;
      
    }
    
}
