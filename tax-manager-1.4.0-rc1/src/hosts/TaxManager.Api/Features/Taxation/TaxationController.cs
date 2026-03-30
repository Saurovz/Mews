using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using TaxManager.Application.Dto;
using TaxManager.Application.Interfaces;
using TaxManager.Extensions;

namespace TaxManager.Features.Taxation;

[ApiController]
[Route("[controller]")]
public class TaxationController : ControllerBase
{
    internal const string ActivitySourceName = "TaxManager.Api.Taxation";
    private readonly ILogger<TaxationController> _logger;
    private readonly ITaxationService _taxationService;
    private readonly IValidator<TaxationCreateDto> _validator;
    public TaxationController(ITaxationService taxationService,
           IValidator<TaxationCreateDto> validator,ILogger<TaxationController> logger)
    {
        _taxationService = taxationService;
        _validator = validator;
        _logger = logger;
    }

    [HttpGet("ListTaxations")]
    [OpenApiOperation("Endpoint for Get All operation",
        "This fetches all the taxation information from the database.")]
    public async Task<ActionResult<IEnumerable<TaxationDto>>> GetAll()
    {
        _logger.LogInformation("Getting all Taxations");
        var taxations =  await _taxationService.GetAllTaxationsAsync();
        return Ok(taxations);
    }
    
    [HttpGet("{code}")]
    public async Task<ActionResult<TaxationDto>> GetByCode(string code)
    {
        _logger.LogInformation("Getting Taxation with code: {Code}", code);
        var taxationDto = await _taxationService.GetTaxationByCodeAsync(code);
        return Ok(taxationDto);
    }

    [HttpPost("AddTaxation")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] TaxationCreateDto taxationCreateDto)
    {
        //Input Validation
        var validationResult = await _validator.ValidateAsync(taxationCreateDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(ValidationExtensions.GetValidationProblems(validationResult));
        }
        //Process request in Service Layer
        var createdTaxation = await _taxationService.CreateTaxationAsync(taxationCreateDto);

        _logger.LogInformation("Created a Taxation resource with value: {Value}", taxationCreateDto);
        return Ok(createdTaxation);
    }

    [HttpGet("GetTaxRates")]
    public async Task<IEnumerable<TaxRateDto>> GetTaxRates()
    {
        return await _taxationService.GetTaxRatesAsync();
    }
    
    [HttpGet("GetStrategies")]
    public IEnumerable<StrategyDto> GetStrategies()
    {
        return _taxationService.GetStrategies();
    }
    
    [HttpGet("GetCurrencies")]
    public IEnumerable<CurrencyDto> GetCurrencies()
    {
        return _taxationService.GetCurrencies();
    }
    
    [HttpGet("GetTimeZones")]
    public async Task<IEnumerable<TimeZoneDto>> GetTimeZones()
    {
        return await _taxationService.GetTimeZonesAsync();
    }
    
    [HttpGet("GetTaxationsByCountryId/{countryId}")]
    public async Task<IEnumerable<TaxationDto>> GetTaxationsByCountryId(int countryId)
    {
        return await _taxationService.GetTaxationsByCountryIdAsync(countryId);
    }
}
