namespace Mews.Job.Scheduler.Aspire.IntegrationTests.Helpers;

public static class DatabaseHelpers
{
    public static void EnsureDatabaseIsCreated()
    {
        using var dbContext = IntegrationTests.DbContextFactory.CreateDbContext();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }
}
