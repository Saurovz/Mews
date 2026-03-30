using System.Diagnostics;

namespace Mews.Job.Scheduler.Core.EntityFrameworkCore.Observability;

public static class EfCoreDiagnostics
{
    public const string ActivitySourceName = "Mews.Job.Scheduler.Core.EntityFrameworkCore";
    public static readonly ActivitySource Source = new ActivitySource(ActivitySourceName);
}
