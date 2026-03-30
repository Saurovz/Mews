using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NUnit.Framework;
using TaxManager.Application.Common;
using TaxManager.Application.Common.Exception;
using TaxManager.Application.Dto;
using TaxManager.Application.Interfaces;
using TaxManager.Application.Mappings;
using TaxManager.Application.Services;
using TaxManager.Domain.Entities;
using TaxManager.Domain.Enums;
using TaxManager.Domain.Interfaces;
using TaxManager.EntityFrameworkCore.Data;
using TaxManager.EntityFrameworkCore.Persistence;
using TaxManager.Extensions;
using TaxManager.Features.Taxation;

namespace TaxManager.UnitTests.Features.Taxations;

[TestFixture]
public class TaxationsTests
{
    private ServiceProvider _serviceProvider;

    private static string[] _specialCharacters =
    [
        "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "_", "+", "[", "]", "{", "}", "|",
        @"\", ":", ";", "'", "\"", ",", ".", "<", ">", "/", "?", "`", "~"
    ];

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();

        // Using In-Memory database for testing
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("TestDb"));
        services.AddScoped<ITaxationRepository, TaxationRepository>();
        services.AddScoped<ICountryRepository, CountryRepository>();
        services.AddScoped<ITaxationService, TaxationService>();
        services.AddScoped<TaxationController>();
        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddScoped<IValidator<TaxationCreateDto>, TaxationCreateDtoValidator>();
        services.AddScoped<IValidator<TaxationTaxRateDto>, TaxationTaxRateDtoValidator>();

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
    public async Task Taxation_Service_Create_Empty_Code_Returns_Error()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();

        try
        {
            var taxation = new TaxationCreateDto("", 1, "");
            await service.CreateTaxationAsync(taxation);
        }
        catch (Exception e)
        {
            Assert.That(e.GetType(), Is.EqualTo((typeof(ArgumentException))));
            Assert.That(e.Message, Is.EqualTo("Taxation code cannot be empty (Parameter 'taxationCode')"));
        }
    }

    [Test]
    public async Task Taxation_Service_GetAll_Empty_Returns_Error()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();
        var dbContext = scopedServices.GetRequiredService<AppDbContext>();
        dbContext.Database.EnsureDeleted();

        try
        {
            await service.GetAllTaxationsAsync();
        }
        catch (Exception e)
        {
            Assert.That(e.GetType(), Is.EqualTo((typeof(NotFoundException))));
            Assert.That(e.Message, Is.EqualTo("No Items found."));
        }
    }

    [Test]
    public async Task Taxation_Controller_GetAll_Logs_Message()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var controller = scopedServices.GetRequiredService<TaxationController>();
        var loggerCollector = scopedServices.GetRequiredService<FakeLogCollector>();

        await controller.GetAll();

        Assert.That(loggerCollector.LatestRecord.Message, Is.EqualTo("Getting all Taxations"));
    }

    [Test]
    public async Task Taxation_Service_GetAll_Adds_To_Cache()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();
        var cacheService = scopedServices.GetRequiredService<ICacheService>();

        await service.GetAllTaxationsAsync();
        var cacheResult = cacheService.ExistsAsync(CacheKeys.TaxationAll);

        Assert.That(cacheResult.Result, Is.True);
    }

    [Test]
    public async Task Taxation_Service_Create_Clears_Cache()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();
        var cacheService = scopedServices.GetRequiredService<ICacheService>();

        await service.GetAllTaxationsAsync();
        var cacheResult = cacheService.ExistsAsync(CacheKeys.TaxationAll);
        Assert.That(cacheResult.Result, Is.True);
        var taxation = new TaxationCreateDto("test", 1, "test");
        taxation.TaxationTaxRates.Add(new TaxationTaxRateDto(Guid.Empty, 1, 1, null, 10, "USD",
            DateTime.Now, DateTime.Now.AddDays(1), "America/Chicago", "America/Chicago"));
        await service.CreateTaxationAsync(taxation);
        cacheResult = cacheService.ExistsAsync(CacheKeys.TaxationAll);

        Assert.That(cacheResult.Result, Is.False);
    }

    [Test]
    public async Task Taxation_Service_GetAll_Pulls_From_Cache()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();
        var cacheService = scopedServices.GetRequiredService<ICacheService>();
        var loggerCollector = scopedServices.GetRequiredService<FakeLogCollector>();

        //First call to add to cache
        await service.GetAllTaxationsAsync();

        //Get from cache
        await service.GetAllTaxationsAsync();
        var cacheResult = cacheService.ExistsAsync(CacheKeys.TaxationAll);

        Assert.That(cacheResult.Result, Is.True);
        Assert.That(loggerCollector.LatestRecord.Message, Is.EqualTo("Found taxations from cache"));
    }
    
    [TestCase(1)]
    public async Task Taxation_Service_GetTaxationsByCountryId_Adds_To_Cache(int countryId)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();
        var cacheService = scopedServices.GetRequiredService<ICacheService>();

        await service.GetTaxationsByCountryIdAsync(countryId);
        var cacheResult = cacheService.ExistsAsync(CacheKeys.TaxationByCountryId(countryId));

        Assert.That(cacheResult.Result, Is.True);
    }
    
    
    [TestCase(1)]
    public async Task Taxation_Service_GetTaxationsByCountryId_Pulls_From_Cache(int countryId)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();
        var cacheService = scopedServices.GetRequiredService<ICacheService>();
        var loggerCollector = scopedServices.GetRequiredService<FakeLogCollector>();

        //First call to add to cache
        await service.GetTaxationsByCountryIdAsync(countryId);

        //Get from cache
        await service.GetTaxationsByCountryIdAsync(countryId);
        var cacheResult = cacheService.ExistsAsync(CacheKeys.TaxationByCountryId(countryId));

        Assert.That(cacheResult.Result, Is.True);
        Assert.That(loggerCollector.LatestRecord.Message, Is.EqualTo($"Found taxations for country {countryId} from cache"));
    }
    
    [TestCase(999)]
    public async Task Taxation_Service_GetTaxationsByCountryId_Empty_Returns_Error(int countryId)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();
        var dbContext = scopedServices.GetRequiredService<AppDbContext>();
        dbContext.Database.EnsureDeleted();

        try
        {
            await service.GetTaxationsByCountryIdAsync(countryId);
        }
        catch (Exception e)
        {
            Assert.That(e.GetType(), Is.EqualTo((typeof(NotFoundException))));
            Assert.That(e.Message, Is.EqualTo("No Items found."));
        }
    }
    
    [Test]
    public async Task Taxation_Service_GetTaxRates_Pulls_From_Cache()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();
        var cacheService = scopedServices.GetRequiredService<ICacheService>();
        var loggerCollector = scopedServices.GetRequiredService<FakeLogCollector>();

        //First call to add to cache
        await service.GetTaxRatesAsync();

        //Get from cache
        await service.GetTaxRatesAsync();
        var cacheResult = cacheService.ExistsAsync(CacheKeys.TaxRatesAll);

        Assert.That(cacheResult.Result, Is.True);
        Assert.That(loggerCollector.LatestRecord.Message, Is.EqualTo("Found tax rates from cache"));
    }

    [TestCase("--")]
    public async Task Taxation_Controller_Create_Failed_Validation_Returns_BadRequest(string code)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var controller = scopedServices.GetRequiredService<TaxationController>();

        var result = await controller.Create(new TaxationCreateDto(code, 1, "test")) as BadRequestObjectResult;
        var resultMessage = ((Dictionary<string, string[]>)result.Value).Values.First().First();
        Assert.That(result.GetType(), Is.EqualTo(typeof(BadRequestObjectResult)));
        Assert.That(result.StatusCode, Is.EqualTo(400));
        Assert.That(resultMessage, Is.EqualTo("Taxation code can contain only alphanumeric characters " +
                                              "and hyphens without spaces and cannot start with a hyphen"));
    }

    [TestCase("")]
    public async Task Taxation_Code_Empty_Fails_Validation(string code)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var validator = scopedServices.GetRequiredService<IValidator<TaxationCreateDto>>();
        var taxation = new TaxationCreateDto(code, 1, "Test Taxation");

        var validationResult = await validator.ValidateAsync(taxation);
        var errors = validationResult.GetValidationProblems();

        Assert.That(validationResult.IsValid, Is.False);
        Assert.That(errors.First().Value.First(), Is.EqualTo("Taxation code cannot be empty"));
    }

    [TestCase(null)]
    public async Task Taxation_Name_Null_Fails_Validation(string name)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var validator = scopedServices.GetRequiredService<IValidator<TaxationCreateDto>>();
        var taxation = new TaxationCreateDto("Tax-1", 1, name);

        var validationResult = await validator.ValidateAsync(taxation);
        var errors = validationResult.GetValidationProblems();

        Assert.That(validationResult.IsValid, Is.False);
        Assert.That(errors.First().Value.First(), Is.EqualTo("Taxation name is required"));
    }

    [TestCase("")]
    public async Task Taxation_Name_Empty_Fails_Validation(string name)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var validator = scopedServices.GetRequiredService<IValidator<TaxationCreateDto>>();
        var taxation = new TaxationCreateDto("Tax-1", 1, name);

        var validationResult = await validator.ValidateAsync(taxation);
        var errors = validationResult.GetValidationProblems();

        Assert.That(validationResult.IsValid, Is.False);
        Assert.That(errors.First().Value.First(), Is.EqualTo("Taxation name cannot be empty"));
    }

    [TestCase(null)]
    public async Task Taxation_Code_Null_Fails_Validation(string code)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var validator = scopedServices.GetRequiredService<IValidator<TaxationCreateDto>>();
        var taxation = new TaxationCreateDto(code, 1, "Test Taxation");

        var validationResult = await validator.ValidateAsync(taxation);
        var errors = validationResult.GetValidationProblems();

        Assert.That(validationResult.IsValid, Is.False);
        Assert.That(errors.First().Value.First(), Is.EqualTo("Taxation code is required"));
    }

    [TestCase("1234567890123456789012345678901234567890")]
    public async Task Taxation_Code_At_Max_Length_Passes_Validation(string code)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var validator = scopedServices.GetRequiredService<IValidator<TaxationCreateDto>>();
        var taxation = new TaxationCreateDto(code, 1, "Test Taxation");

        var validationResult = await validator.ValidateAsync(taxation);
        var errors = validationResult.GetValidationProblems();

        Assert.That(validationResult.IsValid, Is.True);
        Assert.That(errors, Is.Empty);
        Assert.That(validationResult.Errors.Count, Is.EqualTo(0));
    }

    [TestCase("12345678901234567890123456789012345678901")]
    public async Task Taxation_Code_Over_Max_Length_Fails_Validation(string code)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var validator = scopedServices.GetRequiredService<IValidator<TaxationCreateDto>>();
        var taxation = new TaxationCreateDto(code, 1, "Test Taxation");

        var validationResult = await validator.ValidateAsync(taxation);
        var errors = validationResult.GetValidationProblems();

        Assert.That(validationResult.IsValid, Is.False);
        Assert.That(errors.First().Value.First(), Is.EqualTo("Taxation code must be between 2 and 40 characters"));
    }

    [TestCase("12345678901234567890")]
    public async Task Taxation_Name_At_Max_Length_Passes_Validation(string name)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var validator = scopedServices.GetRequiredService<IValidator<TaxationCreateDto>>();
        var taxation = new TaxationCreateDto("Test", 1, name);

        var validationResult = await validator.ValidateAsync(taxation);
        var errors = validationResult.GetValidationProblems();

        Assert.That(validationResult.IsValid, Is.True);
        Assert.That(errors, Is.Empty);
        Assert.That(validationResult.Errors.Count, Is.EqualTo(0));
    }

    [TestCase("12345678901234567890123456789012345678901")]
    public async Task Taxation_Name_Over_Max_Length_Fails_Validation(string name)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var validator = scopedServices.GetRequiredService<IValidator<TaxationCreateDto>>();
        var taxation = new TaxationCreateDto("Test", 1, name);

        var validationResult = await validator.ValidateAsync(taxation);
        var errors = validationResult.GetValidationProblems();

        Assert.That(validationResult.IsValid, Is.False);
        Assert.That(errors.First().Value.First(), Is.EqualTo("Taxation name cannot exceed 40 characters"));
    }

    [TestCase("T-123")]
    public async Task Taxation_Code_With_No_Special_Characters_Passes_Validation(string code)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var validator = scopedServices.GetRequiredService<IValidator<TaxationCreateDto>>();
        var taxation = new TaxationCreateDto(code, 1, "Test");

        var validationResult = await validator.ValidateAsync(taxation);
        var errors = validationResult.GetValidationProblems();

        Assert.That(validationResult.IsValid, Is.True);
        Assert.That(errors, Is.Empty);
        Assert.That(validationResult.Errors.Count, Is.EqualTo(0));
    }

    [TestCaseSource(nameof(_specialCharacters))]
    public async Task Taxation_Code_With_Special_Characters_Fails_Validation(string code)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var validator = scopedServices.GetRequiredService<IValidator<TaxationCreateDto>>();
        var taxation = new TaxationCreateDto(code, 1, "Test");

        var validationResult = await validator.ValidateAsync(taxation);
        var errors = validationResult.GetValidationProblems();

        Assert.That(validationResult.IsValid, Is.False);
        Assert.That(errors.First().Value[1],
            Is.EqualTo(
                "Taxation code can contain only alphanumeric characters and hyphens without spaces and cannot start with a hyphen"));
    }

    [Test]
    public async Task Taxation_Service_Create_With_No_TaxRates_Returns_Error()
    
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();
        var taxation = new TaxationCreateDto("Test", 1, "Test");
        var result = await service.CreateTaxationAsync(taxation);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Does.Contain("At least one Tax Rate is required!"));
    }

    [TestCase("ZZZ")]
    public async Task Taxation_Service_Create_With_TaxRate_FlatRate_Not_Currency_Returns_Error(string valueType)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();
        var taxation = new TaxationCreateDto("Test", 1, "Test");
        taxation.TaxationTaxRates.Add(new TaxationTaxRateDto(
            Guid.Empty, 1, 1, 'A', 10, valueType, DateTime.Now, DateTime.Now.AddDays(1), "America/Chicago", "America/Chicago"));

        var result = await service.CreateTaxationAsync(taxation);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Does.Contain("Tax Rates of type Flat Rate must have a currency valueType"));
    }
    
    [TestCase("USD")]
    public async Task Taxation_Service_Create_With_TaxRate_Relative_Not_Percent_Returns_Error(string valueType)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();
        var taxation = new TaxationCreateDto("Test", 1, "Test");
        taxation.TaxationTaxRates.Add(new TaxationTaxRateDto(
            Guid.Empty, 1, 2, 'A', 10, valueType, DateTime.Now, DateTime.Now.AddDays(1), "America/Chicago", "America/Chicago"));
        var result = await service.CreateTaxationAsync(taxation);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Does.Contain("Tax Rates of type Relative Rate and Relative Rate With Dependencies must have a percentage valueType"));
    }

    [TestCase("USD")]
    public async Task Taxation_Service_Create_With_TaxRate_RelativeWithDependencies_Not_Percent_Returns_Error(
        string valueType)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();
        var taxation = new TaxationCreateDto("Test", 1, "Test");
        taxation.TaxationTaxRates.Add(new TaxationTaxRateDto(
            Guid.Empty, 1, 3, 'A', 10, valueType, DateTime.Now, DateTime.Now.AddDays(1), "America/Chicago", "America/Chicago"));
        var result = await service.CreateTaxationAsync(taxation);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Does.Contain("Tax Rates of type Relative Rate and Relative Rate With Dependencies must have a percentage valueType"));
    }

    [Test]
    public async Task Taxation_Service_Create_With_TaxRate_FlatRate_Has_Dependents_Returns_Error()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();
        var taxation = new TaxationCreateDto("Test", 1, "Test");
        taxation.TaxationTaxRates.Add(new TaxationTaxRateDto(
            Guid.Empty, 1, 1, 'A', 10, "USD", DateTime.Now, DateTime.Now.AddDays(1), "America/Chicago", "America/Chicago"));
        taxation.TaxationTaxRates.First().DependentTaxations = new List<SimpleTaxationDto>();
        taxation.TaxationTaxRates.First().DependentTaxations
            ?.Add(new SimpleTaxationDto { Id = new Guid(), Code = "Test" });
        var result = await service.CreateTaxationAsync(taxation);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Does.Contain("Dependent tax rates are only supported for the strategy Relative Rate With Dependencies"));
    }

    [Test]
    public async Task Taxation_Service_Create_With_TaxRate_Relative_Has_Dependents_Returns_Error()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();
        var taxation = new TaxationCreateDto("Test", 1, "Test");
        taxation.TaxationTaxRates.Add(new TaxationTaxRateDto(
            Guid.Empty, 1, 2, 'A', 10, "%", DateTime.Now, DateTime.Now.AddDays(1), "America/Chicago", "America/Chicago"));
        taxation.TaxationTaxRates.First().DependentTaxations = new List<SimpleTaxationDto>();
        taxation.TaxationTaxRates.First().DependentTaxations
            ?.Add(new SimpleTaxationDto { Id = new Guid(), Code = "Test" });
        var result = await service.CreateTaxationAsync(taxation);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Does.Contain("Dependent tax rates are only supported for the strategy Relative Rate With Dependencies"));
    }

    [Test]
    public async Task Taxation_Service_Create_With_TaxRate_EndDate_Before_Start_Date_Returns_Error()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();
        var taxation = new TaxationCreateDto("Test", 1, "Test");
        taxation.TaxationTaxRates.Add(new TaxationTaxRateDto(
            Guid.Empty, 1, 1, 'A', 10, "USD", DateTime.Now, DateTime.Now.AddDays(-1), "America/Chicago", "America/Chicago"));
        var result = await service.CreateTaxationAsync(taxation);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Does.Contain("Start date with time zone is not earlier than end date for tax rate 'Accommodation'."));
    }

    [TestCase("ZZZ")]
    public async Task Taxation_Service_Create_With_TaxRate_Invalid_StartDateTimeZone_Throws_Error(string timeZone)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();
        var taxation = new TaxationCreateDto("Test", 1, "Test");
        taxation.TaxationTaxRates.Add(new TaxationTaxRateDto(
            Guid.Empty, 1, 1, 'A', 10, "USD", DateTime.Now, DateTime.Now.AddDays(1), timeZone, "America/Chicago"));
        try
        {
            await service.CreateTaxationAsync(taxation);
        }
        catch (Exception e)
        {
            Assert.That(e.Message, Is.EqualTo("Invalid Timezone for 'Accommodation'."));
        }
    }

    [TestCase("ZZZ")]
    public async Task Taxation_Service_Create_With_TaxRate_Invalid_EndDateTimeZone_Throws_Error(string timeZone)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();
        var taxation = new TaxationCreateDto("Test", 1, "Test");
        taxation.TaxationTaxRates.Add(new TaxationTaxRateDto(
            Guid.Empty, 1, 1, 'A', 10, "USD", DateTime.Now, DateTime.Now.AddDays(1), "America/Chicago", timeZone));
        try
        {
            await service.CreateTaxationAsync(taxation);
        }
        catch (Exception e)
        {
            Assert.That(e.Message, Is.EqualTo("Invalid Timezone for 'Accommodation'."));
        }
    }

    [Test]
    public async Task Taxation_Service_GetTimeZones_Adds_To_Cache()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();
        var cacheService = scopedServices.GetRequiredService<ICacheService>();

        await service.GetTimeZonesAsync();
        var cacheResult = cacheService.ExistsAsync(CacheKeys.TimeZonesAll);

        Assert.That(cacheResult.Result, Is.True);
    }

    [Test]
    public async Task Taxation_Service_GetTaxRates_Adds_To_Cache()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();
        var cacheService = scopedServices.GetRequiredService<ICacheService>();

        await service.GetTaxRatesAsync();
        var cacheResult = cacheService.ExistsAsync(CacheKeys.TaxRatesAll);

        Assert.That(cacheResult.Result, Is.True);
    }

    [Test]
    public async Task Taxation_Service_CreateTaxation_Tax_Rate_With_Dependent_Invalid_StartDate_Returns_Error()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();
        var context = scopedServices.GetRequiredService<AppDbContext>();
        var taxation = new TaxationCreateDto("Test", 1, "Test");
        var taxRate = new TaxationTaxRateDto(
            Guid.Empty, 7, 3, 'A', 10, "%", DateTime.Now, DateTime.Now.AddYears(1), "America/Chicago",
            "America/Chicago");
        var dependentTaxation = context.Taxations.First(t => t.Code == "US-CA-TE");
        taxRate.DependentTaxations =
        [
            new SimpleTaxationDto { Id = dependentTaxation.Id, Code = dependentTaxation.Code }
        ];
        taxation.TaxationTaxRates.Add(taxRate);
        
        var result = await service.CreateTaxationAsync(taxation);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Does.Contain($"Start date for tax rate 'Lowest Reduced' is after the dependent taxation: US-CA-TE's 'Lowest Reduced' start date: 2025-06-01T00:00:00-05"));
    }

    [Test]
    public async Task Taxation_Service_CreateTaxation_Tax_Rate_With_Dependent_Invalid_EndDate_Returns_Error()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();
        var context = scopedServices.GetRequiredService<AppDbContext>();
        var taxation = new TaxationCreateDto("Test", 1, "Test");
        var taxRate = new TaxationTaxRateDto(
            Guid.Empty, 7, 3, 'A', 10, "%", DateTime.Now.AddYears(-1), DateTime.Now, "America/Chicago",
            "America/Chicago");
        var dependentTaxation = context.Taxations.First(t => t.Code == "US-CA-TE");
        taxRate.DependentTaxations =
        [
            new SimpleTaxationDto { Id = dependentTaxation.Id, Code = dependentTaxation.Code }
        ];
        taxation.TaxationTaxRates.Add(taxRate);
        
        var result = await service.CreateTaxationAsync(taxation);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Does.Contain($"End date for tax rate 'Lowest Reduced' is before the dependent taxation: " +
                                                $"US-CA-TE's 'Lowest Reduced' end date: 2025-12-31T00:00:00-05"));
    }

    [Test]
    public async Task Taxation_Service_CreateTaxation_Tax_Rate_With_Dependent_Invalid_Tax_Rate_Id_Returns_Error()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();
        var context = scopedServices.GetRequiredService<AppDbContext>();
        var taxation = new TaxationCreateDto("Test", 1, "Test");
        var taxRate = new TaxationTaxRateDto(
            Guid.Empty, 6, 3, 'A', 10, "%", DateTime.Now.AddYears(-1), DateTime.Now.AddYears(1), "America/Chicago",
            "America/Chicago");
        var dependentTaxation = context.Taxations.First(t => t.Code == "US-CA-TE");
        taxRate.DependentTaxations =
        [
            new SimpleTaxationDto { Id = dependentTaxation.Id, Code = dependentTaxation.Code }
        ];
        taxation.TaxationTaxRates.Add(taxRate);
        
        var result = await service.CreateTaxationAsync(taxation);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Does.Contain($"Tax rate: 'Lower Reduced' for Dependent Taxation: 'US-CA-TE' not found."));
    }
    
    [Test]
    public async Task Taxation_Service_CreateTaxation_Tax_Rate_With_Dependent_Invalid_Tax_Rate_Id_And_No_Dates_Returns_Error()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();
        var context = scopedServices.GetRequiredService<AppDbContext>();
        var taxation = new TaxationCreateDto("Test", 1, "Test");
        var taxRate = new TaxationTaxRateDto(
            Guid.Empty, 6, 3, 'A', 10, "%", null, null, "", "");
        var dependentTaxation = context.Taxations.First(t => t.Code == "US-CA-TE");
        taxRate.DependentTaxations =
        [
            new SimpleTaxationDto { Id = dependentTaxation.Id, Code = dependentTaxation.Code }
        ];
        taxation.TaxationTaxRates.Add(taxRate);
        
        var result = await service.CreateTaxationAsync(taxation);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Does.Contain($"Tax rate: 'Lower Reduced' for Dependent Taxation: 'US-CA-TE' not found."));
    }
    
    [Test]
    public async Task Taxation_Service_CreateTaxation_Tax_Rate_With_Multiple_Dependent_Levels_No_Dates_Returns_Success()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();
        var context = scopedServices.GetRequiredService<AppDbContext>();
        var taxation = new TaxationCreateDto("Test", 1, "Test");
        var taxRate = new TaxationTaxRateDto(
            Guid.Empty, 7, 3, 'A', 10, "%", null, null, "", "");
        var dependentTaxation = context.Taxations.First(t => t.Code == "US-CA-TE-TE-1");
        taxRate.DependentTaxations =
        [
            new SimpleTaxationDto { Id = dependentTaxation.Id, Code = dependentTaxation.Code }
        ];
        taxation.TaxationTaxRates.Add(taxRate);
        
        var result = await service.CreateTaxationAsync(taxation);
        Assert.That(result.IsValid, Is.True);
    }
    
    [Test]
    public async Task Taxation_Service_CreateTaxation_Tax_Rate_With_Multiple_Dependent_Levels_Returns_Success()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();
        var context = scopedServices.GetRequiredService<AppDbContext>();
        var taxation = new TaxationCreateDto("Test", 1, "Test");
        var taxRate = new TaxationTaxRateDto(
            Guid.Empty, 7, 3, 'A', 10, "%", DateTime.Now.AddYears(-1), DateTime.Now.AddYears(1), "America/Chicago", "America/Chicago");
        var dependentTaxation = context.Taxations.First(t => t.Code == "US-CA-TE-TE-1");
        taxRate.DependentTaxations =
        [
            new SimpleTaxationDto { Id = dependentTaxation.Id, Code = dependentTaxation.Code }
        ];
        taxation.TaxationTaxRates.Add(taxRate);
        
        var result = await service.CreateTaxationAsync(taxation);
        Assert.That(result.IsValid, Is.True);
    }
    
    [Test]
    public async Task Taxation_Service_CreateTaxation_Tax_Rate_With_Dependent_Different_Timezone_Returns_Success()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();
        var context = scopedServices.GetRequiredService<AppDbContext>();
        //Build test dependent
        var dependent = new Taxation
        {
            Code = "test", CountryId = 1, Name = "test",
            TaxationTaxRates =
            [
                new TaxationTaxRate { TaxRateId = 7, Strategy = Strategy.RelativeRate, Code = 'A', Value = 10, ValueType = "%", 
                    StartDate = new DateTime(2025, 06, 01), EndDate = new DateTime(2025, 06, 10), StartDateTimeZone = "America/Chicago", EndDateTimeZone = "America/Chicago" }
            ]
        };
        var createdDependent = await context.Taxations.AddAsync(dependent);
        await context.SaveChangesAsync();

        //Start date is the same as the dependent, but the timezone is one hour ahead, so the test is valid
        var taxation = new TaxationCreateDto("Test1", 1, "Test1");
        taxation.TaxationTaxRates.Add(new TaxationTaxRateDto(
            Guid.Empty, 7, 3, 'A', 10, "%", new DateTime(2025, 06, 01), new DateTime(2025, 06, 11), "America/New_York", "America/New_York")
            {
                DependentTaxations =
                [
                    new SimpleTaxationDto { Id = createdDependent.Entity.Id, Code = createdDependent.Entity.Code }
                ]
            });
        
        var result = await service.CreateTaxationAsync(taxation);
        Assert.That(result.IsValid, Is.True);
    }
    
    [Test]
    public async Task Taxation_Service_CreateTaxation_Tax_Rate_With_Dependent_Earlier_Timezone_StartDate_Success()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();
        var context = scopedServices.GetRequiredService<AppDbContext>();
        //Build test dependent
        var dependent = new Taxation
        {
            Code = "test", CountryId = 1, Name = "test",
            TaxationTaxRates =
            [
                new TaxationTaxRate { TaxRateId = 7, Strategy = Strategy.RelativeRate, Code = 'A', Value = 10, ValueType = "%", 
                    StartDate = new DateTime(2025, 06, 01), EndDate = new DateTime(2025, 06, 10), StartDateTimeZone = "America/Chicago", EndDateTimeZone = "America/Chicago" }
            ]
        };
        var createdDependent = await context.Taxations.AddAsync(dependent);
        await context.SaveChangesAsync();

        //Start date is the same as the dependent, but the timezone is one hour ahead, so the test is valid
        var taxation = new TaxationCreateDto("Test1", 1, "Test1");
        taxation.TaxationTaxRates.Add(new TaxationTaxRateDto(
            Guid.Empty, 7, 3, 'A', 10, "%", new DateTime(2025, 06, 01), new DateTime(2025, 06, 11), "America/New_York", "America/New_York")
        {
            DependentTaxations =
            [
                new SimpleTaxationDto { Id = createdDependent.Entity.Id, Code = createdDependent.Entity.Code }
            ]
        });
        
        var result = await service.CreateTaxationAsync(taxation);
        Assert.That(result.IsValid, Is.True);
    }
    
    [Test]
    public async Task Taxation_Service_CreateTaxation_Tax_Rate_With_Dependent_Later_Timezone_StartDate_Returns_Error()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();
        var context = scopedServices.GetRequiredService<AppDbContext>();
        //Build test dependent
        var dependent = new Taxation
        {
            Code = "test", CountryId = 1, Name = "test",
            TaxationTaxRates =
            [
                new TaxationTaxRate { TaxRateId = 7, Strategy = Strategy.RelativeRate, Code = 'A', Value = 10, ValueType = "%", 
                    StartDate = new DateTime(2025, 06, 01), EndDate = new DateTime(2025, 06, 10), StartDateTimeZone = "America/Chicago", EndDateTimeZone = "America/Chicago" }
            ]
        };
        var createdDependent = await context.Taxations.AddAsync(dependent);
        await context.SaveChangesAsync();

        //Start date is the same as the dependent, and the timezone is one hour behind, so the test will fail
        var taxation = new TaxationCreateDto("Test1", 1, "Test1");
        taxation.TaxationTaxRates.Add(new TaxationTaxRateDto(
            Guid.Empty, 7, 3, 'A', 10, "%", new DateTime(2025, 06, 01), new DateTime(2025, 06, 10), "America/Denver", "America/Denver")
        {
            DependentTaxations =
            [
                new SimpleTaxationDto { Id = createdDependent.Entity.Id, Code = createdDependent.Entity.Code }
            ]
        });
        
        var result = await service.CreateTaxationAsync(taxation);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Does.Contain($"Start date for tax rate 'Lowest Reduced' is after the dependent taxation: " +
                                                $"{createdDependent.Entity.Code}'s 'Lowest Reduced' start date: 2025-06-01T00:00:00-05"));
    }
    
    [Test]
    public async Task Taxation_Service_CreateTaxation_Tax_Rate_With_Dependent_Later_Timezone_EndDate_Success()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();
        var context = scopedServices.GetRequiredService<AppDbContext>();
        //Build test dependent
        var dependent = new Taxation
        {
            Code = "test", CountryId = 1, Name = "test",
            TaxationTaxRates =
            [
                new TaxationTaxRate { TaxRateId = 7, Strategy = Strategy.RelativeRate, Code = 'A', Value = 10, ValueType = "%", 
                    StartDate = new DateTime(2025, 06, 01), EndDate = new DateTime(2025, 06, 10), StartDateTimeZone = "America/Chicago", EndDateTimeZone = "America/Chicago" }
            ]
        };
        var createdDependent = await context.Taxations.AddAsync(dependent);
        await context.SaveChangesAsync();

        //End date is the same as the dependent, but the timezone is one hour ahead, so the test is valid
        var taxation = new TaxationCreateDto("Test1", 1, "Test1");
        taxation.TaxationTaxRates.Add(new TaxationTaxRateDto(
            Guid.Empty, 7, 3, 'A', 10, "%", new DateTime(2025, 05, 01), new DateTime(2025, 06, 10), "America/Denver", "America/Denver")
        {
            DependentTaxations =
            [
                new SimpleTaxationDto { Id = createdDependent.Entity.Id, Code = createdDependent.Entity.Code }
            ]
        });
        
        var result = await service.CreateTaxationAsync(taxation);
        Assert.That(result.IsValid, Is.True);
    }
    
    [Test]
    public async Task Taxation_Service_CreateTaxation_Tax_Rate_With_Dependent_Earlier_Timezone_EndDate_Returns_Error()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ITaxationService>();
        var context = scopedServices.GetRequiredService<AppDbContext>();
        //Build test dependent
        var dependent = new Taxation
        {
            Code = "test", CountryId = 1, Name = "test",
            TaxationTaxRates =
            [
                new TaxationTaxRate { TaxRateId = 7, Strategy = Strategy.RelativeRate, Code = 'A', Value = 10, ValueType = "%", 
                    StartDate = new DateTime(2025, 06, 01), EndDate = new DateTime(2025, 06, 10), StartDateTimeZone = "America/Chicago", EndDateTimeZone = "America/Chicago" }
            ]
        };
        var createdDependent = await context.Taxations.AddAsync(dependent);
        await context.SaveChangesAsync();

        //End date is the same as the dependent, and the timezone is one hour ahead, so the test will fail
        var taxation = new TaxationCreateDto("Test1", 1, "Test1");
        taxation.TaxationTaxRates.Add(new TaxationTaxRateDto(
            Guid.Empty, 7, 3, 'A', 10, "%", new DateTime(2025, 06, 01), new DateTime(2025, 06, 10), "America/New_York", "America/New_York")
        {
            DependentTaxations =
            [
                new SimpleTaxationDto { Id = createdDependent.Entity.Id, Code = createdDependent.Entity.Code }
            ]
        });
        
        var result = await service.CreateTaxationAsync(taxation);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Does.Contain($"End date for tax rate 'Lowest Reduced' is before the dependent taxation: " +
                                                $"{createdDependent.Entity.Code}'s 'Lowest Reduced' end date: 2025-06-10T00:00:00-05"));
    }
    

    //Test for update with dependee succeed
    //Test for update with dependee start date fail
    //Test for update with dependee end date fail
}
