using FluentValidation;
using TaxManager.Application.Dto;

namespace TaxManager.Features.Taxation;

public class TaxationCreateDtoValidator : AbstractValidator<TaxationCreateDto>
{
    public TaxationCreateDtoValidator()
    {
        RuleFor(x => x.Code)
            .NotNull().WithMessage("Taxation code is required")
            .NotEmpty().WithMessage("Taxation code cannot be empty")
            .Length(2, 40).WithMessage("Taxation code must be between 2 and 40 characters")
            .Matches(@"^[a-zA-Z0-9][a-zA-Z0-9\-]+$")
            .WithMessage("Taxation code can contain only alphanumeric characters and hyphens without spaces and cannot start with a hyphen");
        
        RuleFor(x => x.Name)
            .NotNull().WithMessage("Taxation name is required")
            .NotEmpty().WithMessage("Taxation name cannot be empty")
            .MaximumLength(40).WithMessage("Taxation name cannot exceed 40 characters");
    }
}
