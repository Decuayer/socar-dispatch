using FluentValidation;
using SocarDispatch.Domain.Enums;

namespace SocarDispatch.Application.Features.Teams.Commands.UpdateTeamStatus;

public class UpdateTeamStatusCommandValidator : AbstractValidator<UpdateTeamStatusCommand>
{
    public UpdateTeamStatusCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.RequesterId).NotEmpty();
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => Enum.TryParse<TeamStatus>(s, true, out _))
            .WithMessage("Invalid team status value.");
    }
}
