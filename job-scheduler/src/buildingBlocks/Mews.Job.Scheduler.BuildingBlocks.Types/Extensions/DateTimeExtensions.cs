using System.Globalization;

namespace Mews.Job.Scheduler;

public static class DateTimeExtensions
{
    public static DateTime FromIso8601String(this string value)
    {
        return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
    }

    public static string ToIso8601String(this DateTime dateTime, bool includeDate = true, bool includeTime = true, bool includeMilliseconds = true)
    {
        var millisecondsFormat = includeMilliseconds ? ".fffffff" : "";
        var timeFormat = includeTime ? $"THH:mm:ss{millisecondsFormat}Z" : "";
        var dateFormat = includeDate ? "yyyy-MM-dd" : "";
        var fullFormat = $"{dateFormat}{timeFormat}";

        return dateTime.ToString(fullFormat, CultureInfo.InvariantCulture);
    }
}
