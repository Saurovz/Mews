using Mews.Job.Scheduler.BuildingBlocks.Configuration.Serialization;

namespace Mews.Job.Scheduler.Services;

public static class JsonConfiguration
{
    public static IServiceCollection AddJsonConfiguration(this IServiceCollection services)
    {
        services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options => JsonSerializerOptionsConfiguration.Configure(options.JsonSerializerOptions));
        services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options => JsonSerializerOptionsConfiguration.Configure(options.SerializerOptions));

        return services;
    }
}
