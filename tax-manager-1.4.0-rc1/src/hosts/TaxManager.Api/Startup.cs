using FluentValidation;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using TaxManager.Configuration;
using TaxManager.Extensions;
using TaxManager.Services.HealthChecks;
using Serilog;
using TaxManager.Application.Mappings;
using TaxManager.Common.Exception;
using TaxManager.EntityFrameworkCore.Data;


namespace TaxManager;

public class Startup(
    IWebHostEnvironment env,
    IConfigurationManager configuration)
{
    private string EnvironmentName => env.EnvironmentName;
    private bool IsLocalDevEnvironment => EnvironmentName.IsLocalDevelopment();

    public void ConfigureServices(IServiceCollection services)
    {
        //Register AutomapperProfile that exist in a diff assembly
        services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);
        
        services.AddLayers();
        services.AddRedisCache(configuration, EnvironmentName);
        services.AddSingleton<Instrumentation>();
        services.AddControllers(options =>
        {
            options.Filters.Add<GlobalExceptionFilter>();
        });
        services.AddCustomCors();
        services.AddSwagger(EnvironmentName);
        services.AddProblemDetails();
        services.AddMediatR(x => x.RegisterServicesFromAssemblyContaining(typeof(ApiModule)));
        services.AddValidators();
        services.AddValidatorsFromAssemblyContaining(typeof(ApiModule));
        services.AddHealthChecks().AddCheck<AssemblyVersionHealthCheck>(nameof(AssemblyVersionHealthCheck));
        services.AddTelemetry();

        // Uncomment the following line to enable AzureAd authentication
        // !! (local development uses FakeJwtBearer for live environment you need follow the prerequisites) !!
        // Prerequisites for AzureAd authentication:
        // 1. Ensure the "AzureAd" section is properly configured in the application configuration.
        // 2. Prepare the Infrastructure project for EntraID authentication as per the Atlas documentation:
        //    - Infra SDK Auth Documentation: https://mews.atlassian.net/wiki/x/QQKUEg
        //    - Building Block Auth Documentation: https://mews.atlassian.net/wiki/x/yILMH
        // services.AddAtlasAuthentication(configuration, EnvironmentName);
    }

    public void Configure(IApplicationBuilder app)
    {
        if (IsLocalDevEnvironment)
        {
            app.UseDeveloperExceptionPage();
            app.UseStatusCodePages();
            app.UseSwaggerUi();
        }
        else
        {
            app.UseExceptionHandler();
        }

        app.UseSerilogRequestLogging();
        app.UseCors(ApiModuleConstants.CorsPolicy);
        app.UseOpenApi();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = HealthCheckExtensions.WriteResponse
            });
        });
    }
    
    /// <summary>
    /// This is not used as EF DB-migration is added
    /// </summary>
    /// <param name="app"></param>
    public void ConfigureDb(IHost app)
    {
        if (EnvironmentName.IsLocalDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.EnsureCreated(); //Ideal for rapid prototyping with simple schemas
            //For std development: EF migration to be used 
            DataSeeder.SeedData(dbContext);
        }
         

    }
}
