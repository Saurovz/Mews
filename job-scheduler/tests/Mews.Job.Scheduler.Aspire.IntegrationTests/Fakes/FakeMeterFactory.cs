using System.Diagnostics.Metrics;

namespace Mews.Job.Scheduler.Aspire.IntegrationTests.Fakes;

internal sealed class FakeMeterFactory : IMeterFactory
{
    public void Dispose()
    {
    }

    public Meter Create(MeterOptions options)
    {
        return new Meter(name: "test");
    }
}
