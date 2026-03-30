using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Mews.Job.Scheduler.Core.EntityFrameworkCore;

/// <summary>
/// This factory is responsible for creating instances of `JobSchedulerDbContext` for design-time operations.
/// It is primarily used for Entity Framework Core tasks such as generating and applying migrations.
/// Additionally, it is utilized when working with migration bundles.
/// Note: This factory is not used during the application's runtime.
/// </summary>
public class JobSchedulerDesignTimeDbContextFactory : IDesignTimeDbContextFactory<JobSchedulerDbContext>
{
    private const string ConnectionStringName = "JobSchedulerDatabase";

    public JobSchedulerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<JobSchedulerDbContext>();
        var configuration = BuildConfiguration(args);
        var connectionString = GetConnectionString(configuration);
        
        optionsBuilder.UseSqlServer(connectionString,
            sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure();
                sqlOptions.CommandTimeout(3600);
            }
        );

        return new JobSchedulerDbContext(optionsBuilder.Options);
    }

    private static string? GetConnectionString(IConfigurationRoot configuration)
    {
        var connectionString = configuration["connection"] ?? configuration.GetConnectionString(ConnectionStringName);
        
        return connectionString;
    }

    private static IConfigurationRoot BuildConfiguration(string[] args)
    {
        var appSettingsDirPath = Path.Combine(Directory.GetCurrentDirectory(), "../../../hosts/Mews.Job.Scheduler/");
        var appSettingsDirExists = Directory.Exists(appSettingsDirPath);
        var builder = new ConfigurationBuilder();
        
        // AppSettingsDirPath should exist only in local development environment
        if(appSettingsDirExists)
        {
            builder
                .SetBasePath(appSettingsDirPath)
                .AddJsonFile("appsettings.localAspire.json", optional: true);
        }
        
        builder
            .AddCommandLine(args)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}

