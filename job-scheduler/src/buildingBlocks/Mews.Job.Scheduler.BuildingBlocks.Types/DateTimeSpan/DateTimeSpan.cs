using System.Text.RegularExpressions;

namespace Mews.Job.Scheduler.BuildingBlocks.Types.DateTimeSpan;

public readonly struct DateTimeSpan : IComparable
{
    private static readonly Regex Pattern = new Regex("^(-?[0-9]+)M(-?[0-9]+)D(-?[0-9]+)(\\:)(-?[0-9]+)(\\:)(-?[0-9]+)(\\.)(-?[0-9]+)$");

    private static readonly DateTime BaseLine = new DateTime(2000, 1, 1);

    public DateTimeSpan(int months = 0, int days = 0, int hours = 0, int minutes = 0, int seconds = 0, long nanoseconds = 0)
    {
        Months = months;
        Days = days;
        Hours = hours;
        Minutes = minutes;
        Seconds = seconds + (int)(nanoseconds / 1000000000);
        Nanoseconds = (int)(nanoseconds % 1000000000);

        // Test if TimeSpan can be created.
        ToTimeSpan();
    }

    public static DateTimeSpan Zero { get; } = new DateTimeSpan(nanoseconds: 0);

    public int Nanoseconds { get; }

    public int Seconds { get; }

    public int Minutes { get; }

    public int Hours { get; }

    public int Days { get; }

    public int Months { get; }

    public int TotalMilliseconds => (int)ToTimeSpan().TotalMilliseconds;

    public DateTimeSpanPolarity Polarity => BaseLine.Add(this).CompareTo(BaseLine) switch
    {
        -1 => DateTimeSpanPolarity.Negative,
        0 => DateTimeSpanPolarity.Zero,
        1 => DateTimeSpanPolarity.Positive,
        _ => throw new ArgumentOutOfRangeException()
    };

    public static DateTimeSpan? Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var patternMatch = Pattern.Match(value);
        if (!patternMatch.Success) return null;

        var monthsParsed = int.TryParse(patternMatch.Groups[1].Value, out var months);
        var daysParsed = int.TryParse(patternMatch.Groups[2].Value, out var days);
        var hoursParsed = int.TryParse(patternMatch.Groups[3].Value, out var hours);
        var minutesParsed = int.TryParse(patternMatch.Groups[5].Value, out var minutes);
        var secondsParsed = int.TryParse(patternMatch.Groups[7].Value, out var seconds);
        var nanosecondsParsed = int.TryParse(patternMatch.Groups[9].Value, out var nanoseconds);
        var parsingSuccessful = (
            monthsParsed &&
            daysParsed &&
            hoursParsed &&
            minutesParsed &&
            secondsParsed &&
            nanosecondsParsed
        );

        return !parsingSuccessful ? null : new DateTimeSpan(months, days, hours, minutes, seconds, nanoseconds);
    }

    public DateTimeSpan Add(DateTimeSpan other)
    {
        return new DateTimeSpan(
            Months + other.Months,
            Days + other.Days,
            Hours + other.Hours,
            Minutes + other.Minutes,
            Seconds + other.Seconds,
            (long)Nanoseconds + other.Nanoseconds
        );
    }

    public TimeSpan ToTimeSpan()
    {
        // There is no simple way how to convert "logical" month, so a logical month is treated as 31 days.
        return new TimeSpan(31 * Months + Days, Hours, Minutes, Seconds).Add(TimeSpan.FromTicks(Nanoseconds / 100));
    }

    public override string ToString()
    {
        return $"{Months}M{Days}D{Hours}:{Minutes}:{Seconds}.{Nanoseconds}";
    }

    public int CompareTo(object? obj)
    {
        if (obj is DateTimeSpan other)
        {
            return BaseLine.Add(this).CompareTo(BaseLine.Add(other));
        }
        return 0;
    }
}
