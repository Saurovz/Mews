using Mews.Job.Scheduler.Configuration;
using Mews.Job.Scheduler.Core.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Mews.Job.Scheduler.UnitTests.Data;

/// <summary>
/// Factory class that creates JobSchedulerDbContext instances that share the same underlying in-memory database.
/// Every instance of this class creates its own isolated in-memory database.
/// </summary>
public sealed class InMemoryDbContextFactory : IAsyncDisposable
{
    private static readonly IOptions<SqlConfiguration> SqlConfiguration =
        Options.Create(new SqlConfiguration { SchemaName = "test", ConnectionString = "", RetryExecutionStrategy = RetryExecutionStrategy.Default });

    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<JobSchedulerDbContext> _contextOptions;

    private InMemoryDbContextFactory(SqliteConnection connection, DbContextOptions<JobSchedulerDbContext> contextOptions)
    {
        _connection = connection;
        _contextOptions = contextOptions;
    }

    /// <summary>
    /// Creates and sets up an instance with its own isolated in-memory database which will be shared by all DbContext created with
    /// <see cref="CreateContext"/>.
    /// </summary>
    /// <param name="configureContextAsync">Allows you to configure the DbContext of the database, e.g. set up data.</param>
    /// <remarks>
    /// Disposing this instance also deletes the in-memory database after which it cannot be restored.
    /// </remarks>
    /// <returns>Returns a fully initialized factory ready to create new DbContexts.</returns>
    public static async Task<InMemoryDbContextFactory> SetUpInMemoryDatabase(Func<JobSchedulerDbContext, Task>? configureContextAsync = default)
    {
        var factory = await CreateAsync();
        if (configureContextAsync is not null)
        {
            var context = factory.CreateContext();
            await configureContextAsync(context);
            await context.SaveChangesAsync();
        }

        return factory;
    }

    public JobSchedulerDbContext CreateContext() => new(_contextOptions);

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }

    private static async Task<InMemoryDbContextFactory> CreateAsync()
    {
        // Create the in-memory DB connection. The DB exists only as long as the connection is open.
        // When this connection is closed, the DB and all its data and changes are gone.
        var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();

        // Inject the connection in the context creation options.
        var contextOptions = new DbContextOptionsBuilder<JobSchedulerDbContext>()
            .UseSqlite(connection)
            .Options;

        // Create a context to ensure the schema is created from the model mapping.
        await using var context = new JobSchedulerDbContext(contextOptions);
        await context.Database.EnsureCreatedAsync();

        // We need to emulate SQL Server's RowVerions support for each entity that uses it.
        // See https://www.bricelam.net/2020/08/07/sqlite-and-efcore-concurrency-tokens.html
        await context.Database.ExecuteSqlAsync(@$"
CREATE TRIGGER IF NOT EXISTS JobUpdate
AFTER UPDATE ON Job
BEGIN
    UPDATE Job
    SET EntityVersion = EntityVersion + 1
    WHERE rowid = NEW.rowid;
END;");

        return new InMemoryDbContextFactory(connection, contextOptions);
    }
}
