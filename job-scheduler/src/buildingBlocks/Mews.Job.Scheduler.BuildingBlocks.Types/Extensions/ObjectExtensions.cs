namespace Mews.Job.Scheduler.BuildingBlocks.Types;

public static class ObjectExtensions
{
    public static bool InRange<T>(this T value, T? from = null, T? to = null, bool closed = true)
        where T : struct, IComparable
    {
        if ((from is { } fromValue && (value.Precedes(fromValue) || !closed && value.Equals(fromValue))) ||
            (to is { } toValue && (value.Succeeds(toValue) || !closed && value.Equals(toValue))))
        {
            return false;
        }
        return true;
    }

    public static bool Precedes<T>(this T x, T y)
        where T : struct, IComparable
    {
        return x.CompareTo(y) < 0;
    }

    public static bool Succeeds<T>(this T x, T y)
        where T : struct, IComparable
    {
        return x.CompareTo(y) > 0;
    }
}
