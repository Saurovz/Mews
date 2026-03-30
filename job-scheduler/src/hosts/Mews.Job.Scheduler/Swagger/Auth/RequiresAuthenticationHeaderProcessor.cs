using System.Reflection;
using Mews.Job.Scheduler.Common;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace Mews.Job.Scheduler.Swagger.Auth;

public class RequiresAuthenticationHeaderProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        var hasCustomAuth = context.ControllerType.GetCustomAttributes().OfType<RequiresAuthenticationHeaderAttribute>().Any()
                            || context.MethodInfo.GetCustomAttributes().OfType<RequiresAuthenticationHeaderAttribute>().Any();

        if (hasCustomAuth)
        {
            context.OperationDescription.Operation.Security ??= new List<OpenApiSecurityRequirement>();

            var openApiSecurityRequirement = new OpenApiSecurityRequirement();
            openApiSecurityRequirement.Add(HttpConstants.SecuritySchemeName, Array.Empty<string>());
            context.OperationDescription.Operation.Security.Add(openApiSecurityRequirement);
        }

        return true;
    }
}
