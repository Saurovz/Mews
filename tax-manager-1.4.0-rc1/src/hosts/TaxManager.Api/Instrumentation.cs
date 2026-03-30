namespace TaxManager;

using System.Diagnostics;
using System.Diagnostics.Metrics;

/// <summary>
/// The Instrumentation class is a singleton that bootstraps and manages various global instrumentation details.
/// It focuses primarily on metrics. It centralizes metric management, allowing for easy access and disposal.
/// The metric name is accessible as a constant for use with the OpenTelemetry SDK to observe and export metrics.
/// The various metrics are defined as properties and accessible either directly or indirectly through methods
/// depending on the use case. The class also implements IDisposable to ensure proper cleanup.
/// </summary>
public class Instrumentation : IDisposable
{
    internal const string MeterName = "TaxManager.Api";

    private readonly Meter _meter;

    public Instrumentation()
    {
        var version = typeof(Instrumentation).Assembly.GetName().Version?.ToString();
        _meter = new Meter(MeterName, version);

        // Define the various metrics we want to create
        // You'll notice the unit is {dummys}, this is a unit we are describing
        // It would appear as dummy/second in various tooling
        // It is better however to use the UCUM standard for units
        // You can read more about best practice here - https://learn.microsoft.com/en-us/dotnet/core/diagnostics/metrics-instrumentation#best-practices-3
        DummyDeleteCounter = _meter.CreateCounter<long>("dummy_delete_count", "{dummys}", "Number of times the delete endpoint is called");
        DummyPutCounter = _meter.CreateCounter<long>("dummy_put_count", "{dummys}", "Number of times the delete endpoint is called");
    }

    public Counter<long> DummyDeleteCounter { get; }
    
    // This method is used to increase the value of DummyDeleteCounter
    // it is a multidimensional counter, using an id as an attribute.
    // Be careful about adding too many dimensions to a counter as it can
    // cause a high cardinality which can be difficult to manage.
    public void IncreaseDummyPutCounter(int id) => DummyPutCounter.Add(1,
        new KeyValuePair<string, object?>("dummy.id", id));
    private Counter<long> DummyPutCounter { get; }
    
    // There are lots of other types of metrics you can create, such as histograms, gauges, etc.
    // Refer to the otel documentation for more information.
    
    // This is a great .net otel sdk primer for metrics - https://www.mytechramblings.com/posts/getting-started-with-opentelemetry-metrics-and-dotnet-part-2/
    
    public void Dispose()
    {
        _meter.Dispose();
    }
}
