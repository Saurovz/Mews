using Mews.Job.Scheduler.BuildingBlocks.Types.DateTimeSpan;

namespace Mews.Job.Scheduler.UnitTests;

public static class JobHelpers
{
    public static DateTime GetNextStartUtc(DateTime startUtc, DateTimeSpan period, DateTime nowUtc, bool jobTimeoutRetryDisabled, int timeoutRetryCount, bool lastExecutionTimedOut)
    {
        var timeoutRetryDisabled = jobTimeoutRetryDisabled || Domain.Jobs.Job.TimeoutRetryAttemptLimit == 0;
        var nextPeriodicStartUtc = GetNextPeriodicalStartUtc(startUtc, period, nowUtc);
        if (!lastExecutionTimedOut || timeoutRetryDisabled)
        {
            return nextPeriodicStartUtc;
        }
        var nextBackedOffStart = GetNextBackedOffStartUtc(nowUtc, timeoutRetryCount);

        return nextPeriodicStartUtc < nextBackedOffStart
            ? nextPeriodicStartUtc
            : nextBackedOffStart;
    }

    private static DateTime GetNextPeriodicalStartUtc(DateTime startUtc, DateTimeSpan period, DateTime nowUtc)
    {
        var periodTicks = period.ToTimeSpan().Ticks;
        var elapsed = nowUtc - startUtc;
        var elapsedPeriodCount = elapsed.Ticks / periodTicks;

        return startUtc.Add(TimeSpan.FromTicks(periodTicks * (elapsedPeriodCount + 1)));
    }

    private static DateTime GetNextBackedOffStartUtc(DateTime nowUtc, int timeoutRetryCount)
    {
        return nowUtc.AddMilliseconds(Domain.Jobs.Job.BackoffInMilliseconds * (int)Math.Pow(2, timeoutRetryCount));
    }
}
