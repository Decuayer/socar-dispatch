using FluentValidation;

namespace SocarDispatch.Application.Features.EmergencyCodes.Commands.CreateEmergencyCode;

public class CreateEmergencyCodeCommandValidator : AbstractValidator<CreateEmergencyCodeCommand>
{
    public CreateEmergencyCodeCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(20).WithMessage("Code cannot exceed 20 characters.");

        RuleFor(x => x.ColorHex)
            .NotEmpty().WithMessage("ColorHex is required.")
            .MaximumLength(10).WithMessage("ColorHex cannot exceed 10 characters.");

        RuleFor(x => x.SeverityLevel)
            .InclusiveBetween(1, 5).WithMessage("SeverityLevel must be between 1 and 5.");

        RuleFor(x => x.Description)
            .MaximumLength(250).WithMessage("Description cannot exceed 250 characters.");
    }
}
