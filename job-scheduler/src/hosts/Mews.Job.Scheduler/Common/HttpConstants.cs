namespace Mews.Job.Scheduler.Common;

internal static class HttpConstants
{
    #region Authentication
    internal const string SecuritySchemeName = "job-scheduler";
    internal const string AccessTokenHeaderName = "x-mews-job-scheduler-access-token";
    #endregion

    #region Observability
    internal const string CfRay = "CF-ray";
    internal const string JobSchedulerCorrelationIdHeaderName = "x-mews-job-scheduler-correlation-id";
    #endregion
}
