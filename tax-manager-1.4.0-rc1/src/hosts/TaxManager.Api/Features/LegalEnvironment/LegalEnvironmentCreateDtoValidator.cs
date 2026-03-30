using FluentValidation;
using TaxManager.Application.Dto;

namespace TaxManager.Features.LegalEnvironment;

public class LegalEnvironmentCreateDtoValidator : AbstractValidator<LegalEnvironmentCreateDto>
{
    public LegalEnvironmentCreateDtoValidator()
    {
        RuleFor(x => x.Code)
            .NotNull().WithMessage("Legal Environment code is required")
            .NotEmpty().WithMessage("Legal Environment code cannot be empty")
            .Length(2, 40).WithMessage("Legal Environment code must be between 2 and 40 characters")
            .Matches(@"^[a-zA-Z0-9][a-zA-Z0-9\-]+$")
            .WithMessage("Legal Environment code can contain only alphanumeric characters and hyphens without spaces " +
                         "and cannot begin with a hyphen");
        
        RuleFor(x => x.Name)
            .NotNull().WithMessage("Legal Environment name is required")
            .NotEmpty().WithMessage("Legal Environment name cannot be empty")
            .MaximumLength(40).WithMessage("Legal Environment name cannot exceed 40 characters");

        RuleFor(x => x.DepositTaxRateMode)
            .InclusiveBetween(0,3).WithMessage("Legal Environment deposit tax rate must be between 0 and 3");


    }
}
