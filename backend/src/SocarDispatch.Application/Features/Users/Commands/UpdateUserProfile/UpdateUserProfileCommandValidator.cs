using FluentValidation;

namespace SocarDispatch.Application.Features.Users.Commands.UpdateUserProfile;

public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Department).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SubRole).MaximumLength(50);
        RuleFor(x => x.AvatarUrl).MaximumLength(500);
    }
}
