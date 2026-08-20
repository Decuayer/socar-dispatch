using FluentValidation;

namespace SocarDispatch.Application.Features.Teams.Commands.AddTeamMember;

public class AddTeamMemberCommandValidator : AbstractValidator<AddTeamMemberCommand>
{
    public AddTeamMemberCommandValidator()
    {
        RuleFor(v => v.TeamId)
            .NotEmpty().WithMessage("TeamId is required.");

        RuleFor(v => v.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(v => v.RequesterId)
            .NotEmpty().WithMessage("RequesterId is required.");
    }
}
