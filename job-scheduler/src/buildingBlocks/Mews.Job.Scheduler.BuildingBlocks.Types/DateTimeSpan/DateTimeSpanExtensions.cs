namespace Mews.Job.Scheduler.BuildingBlocks.Types.DateTimeSpan;

public static class DateTimeSpanExtensions
{
    public static DateTimeSpan? ToNullableDateTimeSpan(this string? value)
    {
        return !string.IsNullOrEmpty(value)
            ? value.ToDateTimeSpan()
            : null;
    }

    public static DateTimeSpan? ToDateTimeSpan(this string value)
    {
        return DateTimeSpan.Parse(value);
    }
}
