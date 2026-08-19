using FluentValidation;

namespace SocarDispatch.Application.Features.Teams.Commands.UpdateTeamLocation;

public class UpdateTeamLocationCommandValidator : AbstractValidator<UpdateTeamLocationCommand>
{
    public UpdateTeamLocationCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.Latitude).InclusiveBetween(-90m, 90m).WithMessage("Latitude must be between -90 and 90.");
        RuleFor(x => x.Longitude).InclusiveBetween(-180m, 180m).WithMessage("Longitude must be between -180 and 180.");
    }
}
