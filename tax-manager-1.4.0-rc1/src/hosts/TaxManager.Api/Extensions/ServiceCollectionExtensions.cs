using Mews.Atlas.OpenTelemetry;
using Microsoft.Identity.Web;
using TaxManager.Configuration;
using NSwag;
using NSwag.Generation.Processors.Security;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using WebMotions.Fake.Authentication.JwtBearer;

using FluentValidation;
using TaxManager.Application.Interfaces;
using TaxManager.EntityFrameworkCore.Persistence;
using TaxManager.Domain.Interfaces;
using TaxManager.Application.Services;
using TaxManager.Features.Taxation;
using TaxManager.Application.Dto;
using TaxManager.Features.Country;
using TaxManager.Features.LegalEnvironment;
using TaxManager.Features.Search;

namespace TaxManager.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures authentication for the application using Azure Active Directory (AzureAd) or FakeJwtBearer for local development scenarios.
    /// - In local development or integration environments, a FakeJwtBearer authentication scheme is used for testing purposes.
    /// - In other environments, Azure Active Directory (AzureAd) is configured if the required settings are available in the configuration.
    /// 
    /// Prerequisites for AzureAd authentication:
    /// 1. Ensure the "AzureAd" section is properly configured in the application configuration.
    /// 2. Prepare the Infrastructure project for EntraID authentication as per the Atlas documentation:
    ///    - Infra SDK Auth Documentation: https://mews.atlassian.net/wiki/x/QQKUEg
    ///    - Building Block Auth Documentation: https://mews.atlassian.net/wiki/x/yILMH
    /// </summary>
    public static IServiceCollection AddAtlasAuthentication(this IServiceCollection services, IConfiguration configuration, string environment)
    {
        if (environment.IsLocalDevelopment())
        {
            services
                .AddAuthentication(FakeJwtBearerDefaults.AuthenticationScheme)
                .AddFakeJwtBearer();
        }
        else
        {
            // If AzureAd is configured, use AzureAd for authentication
            if (configuration.GetSection("AzureAd").Exists())
            {
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("S2S_APPLICATION_CLIENT_ID")))
                {
                    configuration["AzureAd:ClientId"] = Environment.GetEnvironmentVariable("S2S_APPLICATION_CLIENT_ID");
                }
                
                services
                    .AddAuthentication()
                    .AddMicrosoftIdentityWebApi(configuration);      
            }
            else
            {
                throw new InvalidOperationException("(AddAtlasAuthentication): The 'AzureAd' configuration section is missing. Ensure it is properly configured for authentication in non-local environments.");
            }
        }
        
        return services;
    }
    
    public static IServiceCollection AddSwagger(this IServiceCollection services, string environment)
    {
        services.AddEndpointsApiExplorer();
        services.AddOpenApiDocument(c =>
        {
            if (environment.IsLocalDevelopment())
            {
                c.AddSecurity("FakeBearer", [], new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    Name = "Authorization", // Header name
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Description = "Enter 'fakebearer {token}' (no quotes). Example: FakeBearer {\"sub\":\"6b631bb9-8f54-4aa2-bd7a-1197c8f96a63\",\"role\":[\"sub_role\",\"admin\"]}"
                });

                c.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("FakeBearer"));
            }
            
            c.PostProcess = document =>
            {
                document.Info.Version = "v1";
                document.Info.Title = "MEWS TaxManager.Api service API";
                document.Info.Description = "TaxManager.Api service API";
                document.Info.TermsOfService = "None";
            };
        });

        return services;
    }

    public static IServiceCollection AddCustomCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(name: ApiModuleConstants.CorsPolicy,
                builder => { builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); });
        });

        return services;
    }

    public static void AddTelemetry(this IServiceCollection services)
    {
        services
            .AddOpenTelemetry()
            .AddTracing(
                sources:
                [
                    TaxationController.ActivitySourceName,
                    CountryController.ActivitySourceName,
                    LegalEnvironmentController.ActivitySourceName,
                    SearchController.ActivitySourceName
                ],
                configureProvider: builder => builder
                    .AddAspNetCoreInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
            )
            .AddMetrics(
                meters:
                [
                    Instrumentation.MeterName
                ],
                configureProvider: builder => builder
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()

            );
    }

    // Add Redis distributed cache
    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration, string environment)
    {
        if (environment.IsLocalDevelopment())
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("redis-server");
                // options.InstanceName = "SampleInstance_"; // Optional prefix for keys
            });
            // Register the Redis service
            services.AddSingleton<ICacheService, RedisCacheService>();
        }
        return services;
    }
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<TaxationCreateDto>, TaxationCreateDtoValidator>();
        services.AddScoped<IValidator<TaxationTaxRateDto>, TaxationTaxRateDtoValidator>();
        services.AddScoped<IValidator<LegalEnvironmentCreateDto>, LegalEnvironmentCreateDtoValidator>();
        //       .AddScoped<IValidator<otherDto,OtherDtoValidator>();
        return services;
    }

    public static IServiceCollection AddLayers(this IServiceCollection services)
    {
        services.AddScoped<ITaxationRepository, TaxationRepository>()
                .AddScoped<ITaxationService, TaxationService>()
                .AddScoped<ICountryRepository, CountryRepository>()
                .AddScoped<ICountryService, CountryService>()
                .AddScoped<ILegalEnvironmentRepository, LegalEnvironmentRepository>()
                .AddScoped<ILegalEnvironmentService, LegalEnvironmentService>()
                .AddScoped<ISearchService, SearchService>();
    
        
        return services;
    }
    
}
