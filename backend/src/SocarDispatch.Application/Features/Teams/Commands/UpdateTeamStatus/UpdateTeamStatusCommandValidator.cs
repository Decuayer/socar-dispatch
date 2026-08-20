using FluentValidation;

namespace SocarDispatch.Application.Features.Teams.Commands.UpdateTeamStatus;

public class UpdateTeamStatusCommandValidator : AbstractValidator<UpdateTeamStatusCommand>
{
    public UpdateTeamStatusCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.RequesterId).NotEmpty();
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => new[] { "Idle", "Forwarded", "OnScene", "Busy" }
                .Contains(s, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Invalid team status value.");
    }
}
