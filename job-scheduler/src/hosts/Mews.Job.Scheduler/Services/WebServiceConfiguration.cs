using Mews.Job.Scheduler.Common;
using Mews.Job.Scheduler.Swagger.Auth;
using NSwag;

namespace Mews.Job.Scheduler.Services;

public static class WebServiceConfiguration
{
    public static IServiceCollection AddCustomCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(name: ApiModuleConstants.CorsPolicy, builder =>
            {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
        });

        return services;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        
        services.AddOpenApiDocument(c =>
        {
            c.AddSecurity(HttpConstants.SecuritySchemeName, new OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.ApiKey,
                Name = HttpConstants.AccessTokenHeaderName,
                In = OpenApiSecurityApiKeyLocation.Header,
                Description = "Job scheduler token authorization header",
            });

            c.OperationProcessors.Add(new RequiresAuthenticationHeaderProcessor());

            c.PostProcess = document =>
            {
                document.Info.Version = "v1";
                document.Info.Title = "Job Scheduler Service API";
                document.Info.Description = "Job Scheduler Service API";
                document.Info.TermsOfService = "None";
            };
        });

        return services;
    }
}
