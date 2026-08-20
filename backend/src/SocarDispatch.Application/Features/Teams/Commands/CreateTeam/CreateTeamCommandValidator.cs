using FluentValidation;

namespace SocarDispatch.Application.Features.Teams.Commands.CreateTeam;

public class CreateTeamCommandValidator : AbstractValidator<CreateTeamCommand>
{
    public CreateTeamCommandValidator()
    {
        RuleFor(v => v.TeamName)
            .NotEmpty().WithMessage("Team name is required.")
            .Length(3, 100).WithMessage("Team name must be between 3 and 100 characters.");

        RuleFor(v => v.RequesterId)
            .NotEmpty().WithMessage("RequesterId is required.");
    }
}
