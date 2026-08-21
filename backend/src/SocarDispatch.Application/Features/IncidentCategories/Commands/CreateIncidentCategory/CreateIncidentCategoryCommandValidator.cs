using FluentValidation;

namespace SocarDispatch.Application.Features.IncidentCategories.Commands.CreateIncidentCategory;

public class CreateIncidentCategoryCommandValidator : AbstractValidator<CreateIncidentCategoryCommand>
{
    public CreateIncidentCategoryCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(250);
    }
}
