using Aspire.Hosting;
using Mews.Atlas.Aspire.Testing.Components.SqlServer;
using Mews.Atlas.Testing;
using Mews.Job.Scheduler.Core.EntityFrameworkCore;

namespace Mews.Job.Scheduler.Aspire.IntegrationTests;

internal class TestBase : TestFixture
{
    /// <summary>
    /// Runs only once per test class
    /// </summary>
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
    }

    /// <summary>
    /// Runs only once per test class
    /// </summary>
    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
    }
}
