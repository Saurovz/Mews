namespace Mews.Job.Scheduler.BuildingBlocks.Domain.Guids;

public static class SequentialGuid
{
    private static readonly long BaseUtcTicks = new DateTime(1900, 1, 1).Ticks;

    public static Guid Create()
    {
        var nowUtc = DateTime.UtcNow;
        var days = new TimeSpan(nowUtc.Ticks - BaseUtcTicks).Days;
        var milliseconds = nowUtc.TimeOfDay.TotalMilliseconds;

        // Create byte arrays corresponding to new Guid, day difference and total milliseconds.
        // Note that SQL Server is accurate to 1/300th of a millisecond so we divide by 3.333333.
        var guidArray = Guid.NewGuid().ToByteArray();
        var daysArray = BitConverter.GetBytes(days);
        var msecsArray = BitConverter.GetBytes((long)(milliseconds / 3.333333));

        // Update the guid with days and milliseconds while conforming to SQL Servers ordering.
        Array.Reverse(daysArray);
        Array.Reverse(msecsArray);
        Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
        Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);

        return new Guid(guidArray);
    }
}
