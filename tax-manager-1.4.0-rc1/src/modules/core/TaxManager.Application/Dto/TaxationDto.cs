namespace TaxManager.Application.Dto;

public record TaxationDto(
    Guid Id,
    string Code,
    string Name
)
{
    public CountryDto Country { get; init; }
    public List<SubdivisionDto?> Subdivisions  { get; init; }
    public List<TaxationTaxRateDto?> TaxationTaxRates { get; init; } 
}
