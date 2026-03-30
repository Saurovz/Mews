namespace Mews.Job.Scheduler.BuildingBlocks.Configuration.Security;

public static class SecurityHeadersConfiguration
{
    public static IDictionary<string, string> HeaderValues = new Dictionary<string, string>
    {
        { "Content-Security-Policy", "default-src \'self\'" },
        { "Referrer-Policy", "no-referrer" },
        { "Permissions-Policy", "" }
    };
}
