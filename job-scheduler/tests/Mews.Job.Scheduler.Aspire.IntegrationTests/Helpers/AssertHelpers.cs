using Mews.Atlas.Aspire.Testing.Components.SqlServer;
using Mews.Job.Scheduler.Core.EntityFrameworkCore;

namespace Mews.Job.Scheduler.Aspire.IntegrationTests.Helpers;

public static class AssertHelpers
{
    public static async Task JobThatAsync(
        DbContextFactory<JobSchedulerDbContext> dbContextFactory,
        Guid jobId,
        Action<Domain.Jobs.Job> assert,
        CancellationToken token = default)
    {
        await using var context = dbContextFactory.CreateDbContext();
        var job = await context.Jobs.FindAsync(jobId, token);
        Assert.That(job, Is.Not.Null);
        assert(job);
    }
}
