using FluentValidation;
using TaxManager.Application.Dto;

namespace TaxManager.Features.Taxation;

public class TaxationTaxRateDtoValidator : AbstractValidator<TaxationTaxRateDto>
{
    public TaxationTaxRateDtoValidator()
    {
        RuleFor(x => x.Code.HasValue ? x.Code.ToString() : "")
            .Matches(@"^[a-zA-Z0-9\-$%]+$")
            .WithMessage(
                "Tax Rate code can contain only alphanumeric characters, hyphens, dollar sign, or a percentage sign");
    }
}
