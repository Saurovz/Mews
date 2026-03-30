namespace TaxManager.Application.Dto;

public record TaxationCreateDto(
    string Code,
    int CountryId,
    string Name
)
{
    public List<SubdivisionDto> Subdivisions { get; init; } = new List<SubdivisionDto>();
    public List<TaxationTaxRateDto> TaxationTaxRates { get; init; } = new List<TaxationTaxRateDto>();
};

