using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NUnit.Framework;
using TaxManager.Application.Common;
using TaxManager.Application.Common.Exception;
using TaxManager.Application.Interfaces;
using TaxManager.Application.Mappings;
using TaxManager.Application.Services;
using TaxManager.Domain.Interfaces;
using TaxManager.EntityFrameworkCore.Data;
using TaxManager.EntityFrameworkCore.Persistence;

namespace TaxManager.UnitTests.Features.Country;

[TestFixture]
public class CountryTests
{
    private ServiceProvider _serviceProvider;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();

        // Using In-Memory database for testing
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("TestDb"));
        services.AddScoped<ICountryService, CountryService>();
        services.AddScoped<ICountryRepository, CountryRepository>();
        services.AddSingleton<ICacheService, RedisCacheService>();
        
        services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);
        services.AddLogging(l => l.AddFakeLogging());
        services.AddDistributedMemoryCache();

        _serviceProvider = services.BuildServiceProvider();
        var dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
        DataSeeder.SeedData(dbContext);
    }
    
    [TearDown]
    public void Cleanup()
    {
        var dbContext = _serviceProvider.GetService<AppDbContext>();
        dbContext.Database.EnsureDeleted();
    }
    
    [Test]
    public async Task Country_Service_GetAll_Empty_Throws_Exception()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ICountryService>();
        var dbContext = _serviceProvider.GetService<AppDbContext>();
        dbContext.Database.EnsureDeleted();
        
        try
        {
            await service.GetAllCountriesAsync();
        }
        catch (Exception e)
        {
            Assert.That(e.GetType(), Is.EqualTo((typeof(NotFoundException))));
            Assert.That(e.Message, Is.EqualTo("No Items found."));
        }
    }

    [Test]
    public async Task Country_Service_GetAll_Adds_To_Cache()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ICountryService>();
        var cacheService = scopedServices.GetRequiredService<ICacheService>();

        await service.GetAllCountriesAsync();
        var cacheResult = cacheService.ExistsAsync(CacheKeys.CountryAll);
        
        Assert.That(cacheResult.Result, Is.True);
    }
    
    [Test]
    public async Task Country_Service_GetAll_Pulls_From_Cache()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ICountryService>();
        var cacheService = scopedServices.GetRequiredService<ICacheService>();
        var loggerCollector = scopedServices.GetRequiredService<FakeLogCollector>();

        //First call to add to cache
        await service.GetAllCountriesAsync();
        
        //Get from cache
        await service.GetAllCountriesAsync();
        var cacheResult = cacheService.ExistsAsync(CacheKeys.CountryAll);
        
        Assert.That(cacheResult.Result, Is.True);
        Assert.That(loggerCollector.LatestRecord.Message, Is.EqualTo("Found countries from cache"));
    }

}
