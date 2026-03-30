using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using TaxManager.Application.Dto;
using TaxManager.Application.Interfaces;

namespace TaxManager.Features.Country;

[ApiController]
[Route("[controller]")]
public class CountryController : ControllerBase
{
    internal const string ActivitySourceName = "TaxManager.Api.Country";
    private readonly ILogger<CountryController> _logger;
    private readonly ICountryService _countryService;
    
    public CountryController(ICountryService countryService,ILogger<CountryController> logger)
    {
        _countryService = countryService;
        _logger = logger;
    }

    [HttpGet("ListCountries")]
    [OpenApiOperation("Endpoint for Get All operation",
        "This fetches all the country information from the database.")]
    public async Task<ActionResult<IEnumerable<CountryDto>>> GetAll()
    {
        var countries =  await _countryService.GetAllCountriesAsync();
        return Ok(countries);
    }
   
    [HttpGet("GetSubdivisionsByCountryId/{countryId}")]
    public async Task<ActionResult<IEnumerable<SubdivisionDto>>> GetSubdivisionsByCountryId(int countryId)
    {
        _logger.LogInformation($"Getting Subdivisions by country id: {countryId}");
        var subdivisions = await _countryService.GetSubdivisionsByCountryIdAsync(countryId);
        return Ok(subdivisions);
    }

    [HttpDelete("DeleteSubdivision/{id}")]
    public async Task<ActionResult> DeleteSubdivision(int id)
    {
        await _countryService.DeleteSubdivisionByIdAsync(id);
        return NoContent();
    }
}
