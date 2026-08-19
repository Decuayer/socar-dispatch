using FluentValidation;

namespace SocarDispatch.Application.Features.Incidents.Commands.ChangeIncidentStatus;

public class ChangeIncidentStatusCommandValidator : AbstractValidator<ChangeIncidentStatusCommand>
{
    public ChangeIncidentStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => new[] { "Open", "Assigned", "Resolved", "Canceled" }
                .Contains(s, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Invalid status value.");
    }
}
