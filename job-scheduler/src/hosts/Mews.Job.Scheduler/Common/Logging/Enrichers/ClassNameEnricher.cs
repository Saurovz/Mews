using System.Collections.Concurrent;
using Serilog.Core;
using Serilog.Events;

namespace Mews.Job.Scheduler.Common.Logging.Enrichers;

/// <summary>
/// An <see cref="ILogEventEnricher"/> implementation which puts a class name into "ClassName" property name.
/// </summary>
/// <remarks>
/// It is based on common property "SourceContext" which refers to a class name including namespace.
/// </remarks>
public class ClassNameEnricher : ILogEventEnricher
{
    private const string PropertyName = "ClassName";

    private static readonly ConcurrentDictionary<string, string> SourceContextToClassNamesDictionary = new(StringComparer.Ordinal);

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (!logEvent.TryGetSourceContext(out var sourceContext))
        {
            return;
        }

        var className = SourceContextToClassNamesDictionary.GetOrAdd(sourceContext!, static value =>
        {
            var lastDotIndex = value.LastIndexOf(".", StringComparison.Ordinal);
            return lastDotIndex > -1 ? value.Substring(lastDotIndex + 1) : value;
        });

        var classNameProperty = propertyFactory.CreateProperty(PropertyName, className);
        logEvent.AddPropertyIfAbsent(classNameProperty);
    }
}
