using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mews.Job.Scheduler.BuildingBlocks.Types.Extensions;

public static class ActivityExtensions
{
    /// <summary>
    /// Creates and starts a new Activity object if there is any listener to the Activity events, returns null otherwise.
    /// </summary>
    /// <param name="name">The operation name of the Activity</param>
    /// <param name="parentContext">The parent ActivityContext object to initialize the created Activity object with.
    /// If the context is null, a default ActivityContext will be initialized</param>
    /// <returns>The created Activity object or null if there is no any listener.</returns>
    public static Activity? StartActivity(this ActivitySource source, [CallerMemberName] string name = "", ActivityContext? parentContext = null)
        => source.StartActivity(name, ActivityKind.Internal, parentContext ?? default);
}
