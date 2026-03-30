using System.ComponentModel.DataAnnotations;

namespace TaxManager.Application.Dto;

public record LegalEnvironmentDto
(
    Guid Id,
    string Code,
    string Name,
    int DepositTaxRateMode,
    List<TaxationDto> Taxations
);
