using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using Mews.Job.Scheduler.BuildingBlocks.Types.Extensions;
using Mews.Job.Scheduler.Core.EntityFrameworkCore.Observability;

namespace Mews.Job.Scheduler.Core.EntityFrameworkCore.Helpers;

public static class SqlHelpers
{
    [Obsolete]
    public static async Task<T> CheckedAction<T>(CancellationToken cancellationToken, Func<CancellationToken, Task<T>> action)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            return await action(cancellationToken);
        }
        catch (Exception exception)
        {
            var dispatchInfo = ExceptionDispatchInfo.Capture(exception);
            SqlActionExceptionMapper.Map(dispatchInfo).Throw();
            return default;
        }
    }
    
    public static async Task<T> CheckedAction<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken, [CallerMemberName] string name = "")
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var activity = EfCoreDiagnostics.Source.StartActivity(name, Activity.Current?.Context);

        try
        {
            return await action(cancellationToken);
        }
        catch (Exception exception)
        {
            var dispatchInfo = ExceptionDispatchInfo.Capture(exception);
            SqlActionExceptionMapper.Map(dispatchInfo).Throw();
            return default;
        }
    }
}
