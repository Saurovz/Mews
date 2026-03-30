namespace TaxManager.Application.Dto;

public record LegalEnvironmentCreateDto(
    string Code,
    string Name,
    int DepositTaxRateMode,
    List<Guid> TaxationIds
);

