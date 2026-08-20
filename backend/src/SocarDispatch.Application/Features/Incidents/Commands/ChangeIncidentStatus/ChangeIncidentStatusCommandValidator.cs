using FluentValidation;
using SocarDispatch.Domain.Enums;

namespace SocarDispatch.Application.Features.Incidents.Commands.ChangeIncidentStatus;

public class ChangeIncidentStatusCommandValidator : AbstractValidator<ChangeIncidentStatusCommand>
{
    public ChangeIncidentStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => Enum.TryParse<IncidentStatus>(s, true, out _))
            .WithMessage("Invalid incident status value.");
    }
}
