using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using TaxManager.Application.Common.Exception;
using TaxManager.Application.Dto;
using TaxManager.Application.Interfaces;
using TaxManager.Application.Mappings;
using TaxManager.Application.Services;
using TaxManager.Domain.Interfaces;
using TaxManager.EntityFrameworkCore.Data;
using TaxManager.EntityFrameworkCore.Persistence;
using TaxManager.Extensions;
using TaxManager.Features.LegalEnvironment;

namespace TaxManager.UnitTests.Features.LegalEnvironment;

[TestFixture]
public class LegalEnvironmentTests
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
        services.AddScoped<ILegalEnvironmentRepository, LegalEnvironmentRepository>();
        services.AddScoped<ILegalEnvironmentService, LegalEnvironmentService>();
        services.AddScoped<LegalEnvironmentController>();        
        services.AddScoped<ITaxationRepository, TaxationRepository>();
        services.AddScoped<IValidator<LegalEnvironmentCreateDto>, LegalEnvironmentCreateDtoValidator>();
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
    public async Task LegalEnvironment_Service_Create_Empty_Code_Returns_Error()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ILegalEnvironmentService>();

        try
        {
            var legalEnv = new LegalEnvironmentCreateDto("", "", 0, new List<Guid>());
            await service.CreateLegalEnvironmentAsync(legalEnv);
        }
        catch (Exception e)
        {
            Assert.That(e.GetType(), Is.EqualTo((typeof(ArgumentException))));
            Assert.That(e.Message, Is.EqualTo("Legal Environment code cannot be empty (Parameter 'legalEnvironmentCode')"));
        }
    }

    [Test]
    public async Task LegalEnvironment_Service_GetAll_Empty_Returns_Error()
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var service = scopedServices.GetRequiredService<ILegalEnvironmentService>();
        var dbContext = scopedServices.GetRequiredService<AppDbContext>();
        dbContext.Database.EnsureDeleted();

        try
        {
            await service.GetAllLegalEnvironmentsAsync();
        }
        catch (Exception e)
        {
            Assert.That(e.GetType(), Is.EqualTo((typeof(NotFoundException))));
            Assert.That(e.Message, Is.EqualTo("No Items found."));
        }
    }
    
    [TestCase("--")]
    public async Task LegalEnvironment_Controller_Create_Failed_Validation_Returns_BadRequest(string code)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var controller = scopedServices.GetRequiredService<LegalEnvironmentController>();

        var result = await controller.Create(new LegalEnvironmentCreateDto(code, "test", 0, new List<Guid>())) as BadRequestObjectResult;
        var resultMessage = ((Dictionary<string, string[]>)result.Value).Values.First().First();
        Assert.That(result.GetType(), Is.EqualTo(typeof(BadRequestObjectResult)));
        Assert.That(result.StatusCode, Is.EqualTo(400));
        Assert.That(resultMessage, Is.EqualTo("Legal Environment code can contain only alphanumeric characters " +
                                              "and hyphens without spaces and cannot begin with a hyphen"));
    }

    [TestCase("")]
    public async Task LegalEnvironment_Code_Empty_Fails_Validation(string code)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var validator = scopedServices.GetRequiredService<IValidator<LegalEnvironmentCreateDto>>();
        var legalEnvironment = new LegalEnvironmentCreateDto (code, "Test", 1, new List<Guid>());
        
        var validationResult = await validator.ValidateAsync(legalEnvironment);
        var errors = validationResult.GetValidationProblems();
        
        Assert.That(validationResult.IsValid, Is.False);
        Assert.That(errors.First().Value.First(), Is.EqualTo("Legal Environment code cannot be empty"));
    }
    
    [TestCase(null)]
    public async Task LegalEnvironment_Name_Null_Fails_Validation(string name)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var validator = scopedServices.GetRequiredService<IValidator<LegalEnvironmentCreateDto>>();
        var legalEnvironment = new LegalEnvironmentCreateDto ("ENV-1-new", name, 1, new List<Guid>());
        
        var validationResult = await validator.ValidateAsync(legalEnvironment);
        var errors = validationResult.GetValidationProblems();
        
        Assert.That(validationResult.IsValid, Is.False);
        Assert.That(errors.First().Value.First(), Is.EqualTo("Legal Environment name is required"));
    }
    
    [TestCase("")]
    public async Task LegalEnvironment_Name_Empty_Fails_Validation(string name)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var validator = scopedServices.GetRequiredService<IValidator<LegalEnvironmentCreateDto>>();
        var legalEnvironment = new LegalEnvironmentCreateDto ("ENV-1-new", name, 1, new List<Guid>());
        
        var validationResult = await validator.ValidateAsync(legalEnvironment);
        var errors = validationResult.GetValidationProblems();
        
        Assert.That(validationResult.IsValid, Is.False);
        Assert.That(errors.First().Value.First(), Is.EqualTo("Legal Environment name cannot be empty"));
    }
    
    [TestCase(null)]
    public async Task LegalEnvironment_Code_Null_Fails_Validation(string code)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var validator = scopedServices.GetRequiredService<IValidator<LegalEnvironmentCreateDto>>();
        var legalEnvironment = new LegalEnvironmentCreateDto (code, "Test", 1, new List<Guid>());
        
        var validationResult = await validator.ValidateAsync(legalEnvironment);
        var errors = validationResult.GetValidationProblems();
        
        Assert.That(validationResult.IsValid, Is.False);
        Assert.That(errors.First().Value.First(), Is.EqualTo("Legal Environment code is required"));
    }
    
    [TestCase("1234567890123456789012345678901234567890")]
    public async Task LegalEnvironment_Code_At_Max_Length_Passes_Validation(string code)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var validator = scopedServices.GetRequiredService<IValidator<LegalEnvironmentCreateDto>>();
        var legalEnvironment = new LegalEnvironmentCreateDto(code, "Test", 1, new List<Guid>());
        
        var validationResult = await validator.ValidateAsync(legalEnvironment);
        var errors = validationResult.GetValidationProblems();
        
        Assert.That(validationResult.IsValid, Is.True);
        Assert.That(errors, Is.Empty);
        Assert.That(validationResult.Errors.Count, Is.EqualTo(0));
    }
    
    [TestCase("12345678901234567890123456789012345678901")]
    public async Task LegalEnvironment_Code_Over_Max_Length_Fails_Validation(string code)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var validator = scopedServices.GetRequiredService<IValidator<LegalEnvironmentCreateDto>>();
        var legalEnvironment = new LegalEnvironmentCreateDto (code, "Test", 1, new List<Guid>());
        
        var validationResult = await validator.ValidateAsync(legalEnvironment);
        var errors = validationResult.GetValidationProblems();
        
        Assert.That(validationResult.IsValid, Is.False);
        Assert.That(errors.First().Value.First(), Is.EqualTo("Legal Environment code must be between 2 and 40 characters"));
    }
    
    [TestCase("1234567890123456789012345678901234567890")]
    public async Task LegalEnvironment_Name_At_Max_Length_Passes_Validation(string name)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var validator = scopedServices.GetRequiredService<IValidator<LegalEnvironmentCreateDto>>();
        var legalEnvironment = new LegalEnvironmentCreateDto("Test", name, 1, new List<Guid>());
        
        var validationResult = await validator.ValidateAsync(legalEnvironment);
        var errors = validationResult.GetValidationProblems();
        
        Assert.That(validationResult.IsValid, Is.True);
        Assert.That(errors, Is.Empty);
        Assert.That(validationResult.Errors.Count, Is.EqualTo(0));
    }
    
    [TestCase("12345678901234567890123456789012345678901")]
    public async Task LegalEnvironment_Name_Over_Max_Length_Fails_Validation(string name)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var validator = scopedServices.GetRequiredService<IValidator<LegalEnvironmentCreateDto>>();
        var legalEnvironment = new LegalEnvironmentCreateDto("Test", name, 1, new List<Guid>());
        
        var validationResult = await validator.ValidateAsync(legalEnvironment);
        var errors = validationResult.GetValidationProblems();
        
        Assert.That(validationResult.IsValid, Is.False);
        Assert.That(errors.First().Value.First(), Is.EqualTo("Legal Environment name cannot exceed 40 characters"));
    }
    
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public async Task LegalEnvironment_DepositTaxRateMode_0_thru_3_Passes_Validation(int taxRateMode)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var validator = scopedServices.GetRequiredService<IValidator<LegalEnvironmentCreateDto>>();
        var legalEnvironment = new LegalEnvironmentCreateDto("Test", "Test", taxRateMode, new List<Guid>());
        
        var validationResult = await validator.ValidateAsync(legalEnvironment);
        var errors = validationResult.GetValidationProblems();
        
        Assert.That(validationResult.IsValid, Is.True);
        Assert.That(errors, Is.Empty);
        Assert.That(validationResult.Errors.Count, Is.EqualTo(0));
    }
    
    [TestCase(4)]
    public async Task LegalEnvironment_DepositTaxRateMode_Greater_Than_3_Fails_Validation(int taxRateMode)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var validator = scopedServices.GetRequiredService<IValidator<LegalEnvironmentCreateDto>>();
        var legalEnvironment = new LegalEnvironmentCreateDto("Test", "Test", taxRateMode, new List<Guid>());
        
        var validationResult = await validator.ValidateAsync(legalEnvironment);
        var errors = validationResult.GetValidationProblems();
        
        Assert.That(validationResult.IsValid, Is.False);
        Assert.That(errors.First().Value.First(), Is.EqualTo("Legal Environment deposit tax rate must be between 0 and 3"));
    }
    
    [TestCase("ENV-123")]
    public async Task LegalEnvironment_Code_With_No_Special_Characters_Passes_Validation(string code)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var validator = scopedServices.GetRequiredService<IValidator<LegalEnvironmentCreateDto>>();
        var legalEnvironment = new LegalEnvironmentCreateDto(code, "Test", 1, new List<Guid>());
        
        var validationResult = await validator.ValidateAsync(legalEnvironment);
        var errors = validationResult.GetValidationProblems();
        
        Assert.That(validationResult.IsValid, Is.True);
        Assert.That(errors, Is.Empty);
        Assert.That(validationResult.Errors.Count, Is.EqualTo(0));
    }

    [TestCaseSource(nameof(_specialCharacters))]
    public async Task LegalEnvironment_Code_With_Special_Characters_Fails_Validation(string code)
    {
        using var scope = _serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var validator = scopedServices.GetRequiredService<IValidator<LegalEnvironmentCreateDto>>();
        var legalEnvironment = new LegalEnvironmentCreateDto(code, "Test", 1, new List<Guid>());
        
        var validationResult = await validator.ValidateAsync(legalEnvironment);
        var errors = validationResult.GetValidationProblems();
        
        Assert.That(validationResult.IsValid, Is.False);
        Assert.That(errors.First().Value[1], Is.EqualTo("Legal Environment code can contain only alphanumeric characters and hyphens without spaces and cannot begin with a hyphen"));
    }
}
