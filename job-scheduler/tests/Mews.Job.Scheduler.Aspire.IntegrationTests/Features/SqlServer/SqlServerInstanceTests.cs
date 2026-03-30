using Microsoft.EntityFrameworkCore;

namespace Mews.Job.Scheduler.Aspire.IntegrationTests.Features.SqlServer;

[TestFixture]
internal sealed class SqlServerInstanceHealthCheckTests : TestBase
{
    [Test]
    public async Task HealthCheckSucceeds()
    {
        var result = await HealthCheckAsync();
        Assert.That(result, Is.EqualTo(-1));
    }

    private async Task<int> HealthCheckAsync()
    {
        await using var context = IntegrationTests.DbContextFactory.CreateDbContext();
        var queryString = @"SELECT 1;";
        return await context.Database.ExecuteSqlRawAsync(queryString);
    }
}
