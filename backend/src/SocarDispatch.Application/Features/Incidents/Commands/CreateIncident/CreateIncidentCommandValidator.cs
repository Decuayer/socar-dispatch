using FluentValidation;

namespace SocarDispatch.Application.Features.Incidents.Commands.CreateIncident;

public class CreateIncidentCommandValidator : AbstractValidator<CreateIncidentCommand>
{
    private static readonly string[] AllowedCategories = 
    {
        "Fire",
        "Medical",
        "Security",
        "Environmental",
        "Chemical"
    };

    public CreateIncidentCommandValidator()
    {
        RuleFor(x => x.ReporterId)
            .NotEmpty().WithMessage("ReporterId is required.");
        
        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Incident category is required.")
            .MaximumLength(50).WithMessage("Category cannot exceed 50 characters.")
            .Must(c => AllowedCategories.Contains(c))
            .WithMessage($"Invalid category. Allowed values: {string.Join(", ", AllowedCategories)}");
        
        RuleFor(x => x.EmergencyCode)
            .NotEmpty().WithMessage("Emergency code is required.")
            .MaximumLength(20).WithMessage("Emergency code cannot exceed 20 characters.");

        // Coordinate Validation (-90 <= Latitude <= 90, -180 <= Longitude <= 180)
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90m, 90m)
            .WithMessage("Latitude must be between -90 and 90 degrees.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180m, 180m)
            .WithMessage("Longitude must be between -180 and 180 degrees.");

        // Media Attachments Validation
        RuleForEach(x => x.MediaAttachments).ChildRules(media =>
        {
            media.RuleFor(m => m.MediaUrl)
                .NotEmpty().WithMessage("MediaUrl cannot be empty.");
        });
    }
}
