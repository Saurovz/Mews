using System.Diagnostics;
using Aspire.Hosting;
using Mews.Atlas.Aspire.Testing;
using Mews.Atlas.Aspire.Testing.Components.SqlServer;
using Mews.Atlas.Aspire.Testing.Components.SystemUnderTest;
using Mews.Job.Scheduler.Aspire.AppHost;
using Mews.Job.Scheduler.Core.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Mews.Job.Scheduler.Aspire.IntegrationTests;

/// <summary>
/// This setup fixture is used before and after all tests inside the assembly.
/// This can be restricted to a specific namespace and all its sub-namespaces
/// </summary>
[SetUpFixture]
public sealed class IntegrationTests
{
    private static IDistributedTestingEnvironment? _distributedTestingEnvironment;
    private static DbContextFactory<JobSchedulerDbContext>? _dbContextFactory;
    private static IDbContextHelpers? _dbContextHelpers;

    public static DistributedApplication AspireDistributedApp =>
        _distributedTestingEnvironment?.AspireDistributedApp
        ?? throw new InvalidOperationException(
            "AspireDistributedApp is not initialized. Ensure GlobalSetUp has been executed successfully.");

    public static DbContextFactory<JobSchedulerDbContext> DbContextFactory =>
        _dbContextFactory ?? throw new InvalidOperationException(
            "DbContextFactory has not been initialized. Ensure GlobalSetUp has been executed successfully.");

    [OneTimeSetUp]
    public async Task GlobalSetUp()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());

        try
        {
            var serviceProvider = BuildServiceProvider();
            InitializeDependencies(serviceProvider);
            await InitializeEnvironmentAsync();
            InitializeDbContextFactory();
        }
        catch (Exception e)
        {
            Trace.WriteLine($"Global setup failed: {e.Message}\n{e.StackTrace}");

            if (_distributedTestingEnvironment != null) await _distributedTestingEnvironment.CleanupAsync();

            throw;
        }
    }

    [OneTimeTearDown]
    public async Task GlobalTearDown()
    {
        try
        {
            Trace.Flush();
            if (_distributedTestingEnvironment != null) await _distributedTestingEnvironment.CleanupAsync();
        }
        catch (Exception e)
        {
            Trace.WriteLine($"Global teardown failed: {e.Message}\n{e.StackTrace}");
        }
    }

    private static void InitializeDependencies(ServiceProvider serviceProvider)
    {
        _distributedTestingEnvironment = serviceProvider.GetRequiredService<IDistributedTestingEnvironment>();
        _dbContextHelpers = serviceProvider.GetRequiredService<IDbContextHelpers>();
    }

    private static async Task InitializeEnvironmentAsync()
    {
        var serverTestConfiguration = CreateServerTestConfiguration();
        await _distributedTestingEnvironment!.InitializeAsync(serverTestConfiguration);
    }

    private static ServerTestConfiguration CreateServerTestConfiguration()
    {
        return new ServerTestConfiguration(
            new[] { "ASPIRE_WORKFLOW=test" },
            new SqlServerConfiguration(
                AspireAppHostConfiguration.SqlServerResourceName,
                AspireAppHostConfiguration.SqlServerDatabaseName
            ),
            new DatabaseMigratorServiceConfiguration(
                AspireAppHostConfiguration.MigrationProjectResourceName
            ),
            new SystemUnderTestConfiguration(AspireAppHostConfiguration.ProjectResourceName)
        );
    }

    private static void InitializeDbContextFactory()
    {
        if (_distributedTestingEnvironment?.SqlServerConnectionInfo == null)
        {
            throw new InvalidOperationException("SQL connection string is not available.");
        }
        _dbContextFactory = _dbContextHelpers!.CreateDbContextFactory<JobSchedulerDbContext>(_distributedTestingEnvironment.SqlServerConnectionInfo.DatabaseConnectionString);
        _dbContextHelpers.EnsureDatabaseCreated(_dbContextFactory);
    }

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IDistributedTestingEnvironment, DistributedTestingEnvironment>();
        services.AddSingleton<IDbContextHelpers, DbContextHelpers>();
        return services.BuildServiceProvider();
    }
}
