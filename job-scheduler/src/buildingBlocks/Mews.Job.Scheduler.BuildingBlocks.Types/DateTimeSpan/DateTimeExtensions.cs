namespace Mews.Job.Scheduler.BuildingBlocks.Types.DateTimeSpan;

public static class DateTimeExtensions
{
    public static DateTime Add(this DateTime dateTime, DateTimeSpan span)
    {
        var ticks = span.Nanoseconds / 100;
        var withDays = dateTime.AddMonths(span.Months).AddDays(span.Days);
        return withDays.AddHours(span.Hours).AddMinutes(span.Minutes).AddSeconds(span.Seconds).AddTicks(ticks);
    }
}
