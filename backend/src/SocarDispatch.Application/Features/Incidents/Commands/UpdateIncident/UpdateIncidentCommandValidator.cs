using FluentValidation;

namespace SocarDispatch.Application.Features.Incidents.Commands.UpdateIncident;

public class UpdateIncidentCommandValidator : AbstractValidator<UpdateIncidentCommand>
{
    public UpdateIncidentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.RequesterId).NotEmpty();
        RuleFor(x => x.Category).NotEmpty().MaximumLength(50);
        RuleFor(x => x.EmergencyCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Latitude).InclusiveBetween(-90m, 90m);
        RuleFor(x => x.Longitude).InclusiveBetween(-180m, 180m);
    }
}
