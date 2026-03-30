using TaxManager.Domain.Entities;

namespace TaxManager.Application.Dto;

public record TaxationTaxRateDto(
    Guid Id,
    int TaxRateId,
    int StrategyId,
    char? Code,
    double Value,
    string ValueType,
    DateTime? StartDate,
    DateTime? EndDate,
    string? StartDateTimeZone,
    string? EndDateTimeZone
)
{
    public List<SimpleTaxationDto>? DependentTaxations { get; set; }
}

public record SimpleTaxationDto()
{
    public Guid Id { get; set; }
    public string Code { get; set; }
}
