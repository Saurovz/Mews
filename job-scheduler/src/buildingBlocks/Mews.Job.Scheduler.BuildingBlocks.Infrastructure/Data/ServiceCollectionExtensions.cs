using Mews.Job.Scheduler.BuildingBlocks.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Mews.Job.Scheduler.BuildingBlocks.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDbContextProvider(this IServiceCollection services)
    {
        services.AddTransient(typeof(IEfCoreRepository<,>), typeof(EfCoreRepository<,>));

        return services;
    }

    public static IServiceCollection AddRepository<TRepository, TImplementation>(this IServiceCollection services)
        where TRepository : class
        where TImplementation : class, TRepository
    {
        services.AddScoped<TRepository, TImplementation>();

        return services;
    }
}
