using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TaxManager.EntityFrameworkCore.Data;

/// <summary>
/// This design-time factory is used to generate Db-migrations ONLY and so has no role in application's runtime
/// Without this design-time way of implementation, db migration will not work as it will not resolve the dependencies for repositories and services
/// https://learn.microsoft.com/en-us/ef/core/cli/dbcontext-creation?tabs=dotnet-core-cli
/// Migration to be generated as "...modules/core/TaxManager.EntityFrameworkCore>dotnet ef migrations add InitialCreate" 
/// </summary>
public class AppDbContextFactory: IDesignTimeDbContextFactory<AppDbContext>
{
    private const string ConnectionStringName = "TaxManagerDatabase";
    public AppDbContext CreateDbContext(string[] args)
    {
       
        var configuration = BuildConfiguration(args);
        var connectionString = configuration["connection"] ?? configuration.GetConnectionString(ConnectionStringName);
        Console.WriteLine("The conn string: {0}", connectionString);
        // if (string.IsNullOrEmpty(connectionString))
        // {
        //     // Fallback for Aspire's connection handling
        //     connectionString = "Server=(localdb)\\mssqllocaldb;Database=TaxManagerDb;Trusted_Connection=True;";
        // }
        
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString,
            sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure();
                sqlOptions.CommandTimeout(3600);
            }
        );
        return new AppDbContext(optionsBuilder.Options);
    }
    
    private static IConfigurationRoot BuildConfiguration(string[] args)
    {
       
        // Get the current working directory (where the app is running from)
        var workDir = Directory.GetCurrentDirectory();
        // Moves up 3 levels; this is done to get the file: appsettings.localAspire.json
        var appSettingsDirPath = Path.Combine(workDir, "../../../hosts/TaxManager.Api/");
        //var projectRoot = Path.GetFullPath(appSettingsDirPath);  // Resolves to absolute path
       // Console.WriteLine("The absolute path  {0}", projectRoot); C:\TEK_MEWS\MEWS\tax-manager\src\hosts\TaxManager.Api\
        var appSettingsDirExists = Directory.Exists(appSettingsDirPath);
        var builder = new ConfigurationBuilder();
        
       // AppSettingsDirPath should exist only in local development environment
        if(appSettingsDirExists)
        {
            builder
                .SetBasePath(appSettingsDirPath)
                .AddJsonFile("appsettings.localAspire.json", optional: true, reloadOnChange: true);
        }
        
        builder
            .AddCommandLine(args)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}
