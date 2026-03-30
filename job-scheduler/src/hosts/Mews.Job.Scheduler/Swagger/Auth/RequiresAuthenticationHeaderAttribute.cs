namespace Mews.Job.Scheduler.Swagger.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequiresAuthenticationHeaderAttribute : Attribute
{
}
