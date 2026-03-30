using NJsonSchema.Annotations;
using Serilog.Events;

namespace Mews.Job.Scheduler.Common.Logging;

internal static class SerilogExtensions
{
    public static bool TryGetSourceContext(this LogEvent logEvent, [CanBeNull] out string? sourceContext)
    {
        if (logEvent.Properties.TryGetValue("SourceContext", out var value)
            && value is ScalarValue { Value: string str }
            && !string.IsNullOrEmpty(str))
        {
            sourceContext = str;
            return true;
        }

        sourceContext = null;

        return false;
    }
}
