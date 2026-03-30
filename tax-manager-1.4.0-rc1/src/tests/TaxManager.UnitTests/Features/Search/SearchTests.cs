using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using TaxManager.Application.Common.Exception;
using TaxManager.Application.Interfaces;
using TaxManager.Application.Mappings;
using TaxManager.Application.Services;
using TaxManager.Domain.Interfaces;
using TaxManager.EntityFrameworkCore.Data;
using TaxManager.EntityFrameworkCore.Persistence;
using TaxManager.Features.Search;

namespace TaxManager.UnitTests.Features.Search;

[TestFixture]
public class SearchTests
{
    private ServiceProvider _serviceProvider;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();

        // Using In-Memory database for testing
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("TestDb"));
        services.AddScoped<ITaxationRepository, TaxationRepository>();
        services.AddScoped<ITaxationService, TaxationService>();
        services.AddScoped<ILegalEnvironmentRepository, LegalEnvironmentRepository>();
        services.AddScoped<ILegalEnvironmentService, LegalEnvironmentService>();
        services.AddScoped<ICountryRepository, CountryRepository>();
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<SearchController>();
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
    
    [TestCase("", "")]
    public async Task Search_Code_And_Name_Empty_Throws_Exception(string code, string name)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var controller = scopedServices.GetRequiredService<SearchController>();

        try
        {
            await controller.Search(code, name);
        }
        catch (Exception e)
        {
            Assert.That(e.GetType(), Is.EqualTo((typeof(NotFoundException))));
            Assert.That(e.Message, Is.EqualTo("Search code and name are empty."));
        }
    }
    
    [TestCase("asdf", "")]
    public async Task Search_Code_And_Name_No_Results_Throws_Exception(string code, string name)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var searchService = scopedServices.GetRequiredService<ISearchService>();

        try
        {
            await searchService.Search(code, name);
        }
        catch (Exception e)
        {
            Assert.That(e.GetType(), Is.EqualTo((typeof(NotFoundException))));
            Assert.That(e.Message, Is.EqualTo("No results found."));
        }
    }
    
    [TestCase("CH", "")]
    public async Task Search_Code_Returns_Single_Results(string code, string name)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var searchService = scopedServices.GetRequiredService<ISearchService>();
        
        var searchResult = await searchService.Search(code, name);

        Assert.That(searchResult.Taxations.Count(), Is.EqualTo(1));
        Assert.That(searchResult.Taxations.First().Name, Is.EqualTo("Taxation flat rate"));
        Assert.That(searchResult.LegalEnvironments.Count(), Is.EqualTo(1));
        Assert.That(searchResult.LegalEnvironments.First().Name, Is.EqualTo("United States - Alabama State - Cherokee County - Centre"));
    }
    
    [TestCase("US", "")]
    public async Task Search_Code_Returns_Two_Results(string code, string name)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var searchService = scopedServices.GetRequiredService<ISearchService>();
        
        var searchResult = await searchService.Search(code, name);

        Assert.That(searchResult.Taxations.Count(), Is.EqualTo(3));
        Assert.That(searchResult.Taxations.First().Name, Is.EqualTo("Taxation relative"));
        Assert.That(searchResult.LegalEnvironments.Count(), Is.EqualTo(1));
        Assert.That(searchResult.LegalEnvironments.First().Name, Is.EqualTo("United States - Alabama State - Cherokee County - Centre"));
    }
    
    [TestCase("CH-2018", "")]
    public async Task Search_Code_Returns_Single_Taxation_Result(string code, string name)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var searchService = scopedServices.GetRequiredService<ISearchService>();
        
        var searchResult = await searchService.Search(code, name);

        Assert.That(searchResult.Taxations.Count(), Is.EqualTo(1));
        Assert.That(searchResult.Taxations.First().Name, Is.EqualTo("Taxation flat rate"));
        Assert.That(searchResult.LegalEnvironments.Count(), Is.EqualTo(0));
    }
    
    [TestCase("AW-2023", "")]
    public async Task Search_Code_Returns_Single_LegalEnvironment_Result(string code, string name)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var searchService = scopedServices.GetRequiredService<ISearchService>();
        
        var searchResult = await searchService.Search(code, name);

        Assert.That(searchResult.Taxations.Count(), Is.EqualTo(0));
        Assert.That(searchResult.LegalEnvironments.Count(), Is.EqualTo(1));
        Assert.That(searchResult.LegalEnvironments.First().Name, Is.EqualTo("Aruba"));
    }
    
    [TestCase("", "Taxation relative")]
    public async Task Search_Name_Returns_Single_Taxation_Result(string code, string name)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var searchService = scopedServices.GetRequiredService<ISearchService>();
        
        var searchResult = await searchService.Search(code, name);

        Assert.That(searchResult.Taxations.Count(), Is.EqualTo(1));
        Assert.That(searchResult.Taxations.First().Name, Is.EqualTo("Taxation relative"));
        Assert.That(searchResult.LegalEnvironments.Count(), Is.EqualTo(0));
    }
    
    [TestCase("", "Andorra")]
    public async Task Search_Name_Returns_Single_LegalEnvironment_Result(string code, string name)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var searchService = scopedServices.GetRequiredService<ISearchService>();
        
        var searchResult = await searchService.Search(code, name);

        Assert.That(searchResult.Taxations.Count(), Is.EqualTo(0));
        Assert.That(searchResult.LegalEnvironments.Count(), Is.EqualTo(1));
        Assert.That(searchResult.LegalEnvironments.First().Name, Is.EqualTo("Andorra"));
    }
}
