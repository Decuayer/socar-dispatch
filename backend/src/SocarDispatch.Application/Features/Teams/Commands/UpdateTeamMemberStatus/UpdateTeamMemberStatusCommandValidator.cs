using FluentValidation;
using SocarDispatch.Domain.Enums;

namespace SocarDispatch.Application.Features.Teams.Commands.UpdateTeamMemberStatus;

public class UpdateTeamMemberStatusCommandValidator : AbstractValidator<UpdateTeamMemberStatusCommand>
{
    public UpdateTeamMemberStatusCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.TargetUserId).NotEmpty();
        RuleFor(x => x.RequesterId).NotEmpty();
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => Enum.TryParse<TeamMemberStatus>(s, true, out _))
            .WithMessage("Invalid team member status value. Allowed values: Available, EnRoute, OnScene, Unavailable.");
    }
}
