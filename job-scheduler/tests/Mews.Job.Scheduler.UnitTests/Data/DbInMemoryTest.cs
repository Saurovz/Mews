using Mews.Job.Scheduler.Domain.Jobs;
using Mews.Job.Scheduler.Tests;
using Microsoft.EntityFrameworkCore;

namespace Mews.Job.Scheduler.UnitTests.Data;

[TestFixture]
public sealed class DbInMemoryTest
{
    private InMemoryDbContextFactory _dbContextFactory = null!;

    [SetUp]
    public async Task SetUp()
    {
        _dbContextFactory = await InMemoryDbContextFactory.SetUpInMemoryDatabase(async context =>
            await context.AddRangeAsync(TestData.CreateJob(JobState.InProgress))
        );
    }

    [TearDown]
    public async Task TearDown()
    {
        await _dbContextFactory.DisposeAsync();
    }

    [Test]
    public async Task DbConnectionTest()
    {
        var ctx = _dbContextFactory.CreateContext();

        var jobs = await ctx.Jobs.ToListAsync();

        ClassicAssert.IsNotEmpty(jobs);
    }

    [Test]
    public async Task DbEntityUpdateTest()
    {
        var ctx = _dbContextFactory.CreateContext();
        var job = await ctx.Jobs.FirstAsync();
        var state = JobState.Pending;

        job.State = state;

        Assert.DoesNotThrowAsync(async () =>
        {
            await ctx.SaveChangesAsync();
        });

        var ctx2 = _dbContextFactory.CreateContext();
        var job2 = await ctx2.Jobs.FirstAsync();

        ClassicAssert.IsTrue(job2.State == state);
    }

    [Test]
    public async Task DbEntityConcurrencyTest()
    {
        var ctx = _dbContextFactory.CreateContext();
        var job = await ctx.Jobs.FirstAsync();

        await using (var ctx2 = _dbContextFactory.CreateContext())
        {
            var job2 = await ctx2.Jobs.FirstAsync();
            job2.Data = "UpdatedTestJob";
            await ctx2.SaveChangesAsync();
        }

        job.State = JobState.Pending;
        Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
        {
            await ctx.SaveChangesAsync();
        });
    }
}
