using FluentValidation;

namespace SocarDispatch.Application.Features.IncidentCategories.Commands.UpdateIncidentCategory;

public class UpdateIncidentCategoryCommandValidator : AbstractValidator<UpdateIncidentCategoryCommand>
{
    public UpdateIncidentCategoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(250);
    }
}
