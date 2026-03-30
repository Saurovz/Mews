using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using TaxManager.Application.Dto;
using TaxManager.Application.Interfaces;
using TaxManager.Extensions;

namespace TaxManager.Features.LegalEnvironment;

[ApiController]
[Route("[controller]")]
public class LegalEnvironmentController : ControllerBase
{
    internal const string ActivitySourceName = "TaxManager.Api.LegalEnvironment";
    private readonly ILegalEnvironmentService _legalEnvironmentService;
    private readonly IValidator<LegalEnvironmentCreateDto> _validator;
    
    public LegalEnvironmentController(ILegalEnvironmentService legalEnvironmentService,
        IValidator<LegalEnvironmentCreateDto> validator)
    {
        _legalEnvironmentService = legalEnvironmentService;
        _validator = validator;
    }

    [HttpPost("AddLegalEnvironment")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] LegalEnvironmentCreateDto legalEnvironmentCreateDto)
    {
        //Input Validation
        var validationResult = await _validator.ValidateAsync(legalEnvironmentCreateDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(ValidationExtensions.GetValidationProblems(validationResult));
        }
        //Process request in Service Layer
        var createdLegalEnvironment = await _legalEnvironmentService.CreateLegalEnvironmentAsync(legalEnvironmentCreateDto);
        return CreatedAtAction(nameof(GetByCode), new { code = createdLegalEnvironment.Code }, createdLegalEnvironment);
    }
    
    [HttpGet("ListLegalEnvironments")]
    [OpenApiOperation("Endpoint for Get All operation",
        "This fetches all the legal environment information from the database.")]
    public async Task<ActionResult<IEnumerable<LegalEnvironmentDto>>> GetAll()
    {
        var legalEnvironments =  await _legalEnvironmentService.GetAllLegalEnvironmentsAsync();
        return Ok(legalEnvironments);
    }
    
    [HttpGet("{code}")]
    public async Task<ActionResult<LegalEnvironmentDto>> GetByCode(string code)
    {
        var legalEnvironment = await _legalEnvironmentService.GetLegalEnvironmentByCodeAsync(code);
        return Ok(legalEnvironment);
    }
}
