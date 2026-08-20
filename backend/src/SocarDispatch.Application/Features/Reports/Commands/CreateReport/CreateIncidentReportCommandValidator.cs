using FluentValidation;

namespace SocarDispatch.Application.Features.Reports.Commands.CreateReport;

public class CreateIncidentReportCommandValidator : AbstractValidator<CreateIncidentReportCommand>
{
    public CreateIncidentReportCommandValidator()
    {
        RuleFor(x => x.IncidentId)
            .NotEmpty().WithMessage("Incident ID is required.");

        RuleFor(x => x.ReportedByUserId)
            .NotEmpty().WithMessage("Reporter User ID is required.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Report content is required.")
            .MaximumLength(4000).WithMessage("Report content must not exceed 4000 characters.");

        RuleFor(x => x.MediaUrl)
            .MaximumLength(500).WithMessage("Media URL must not exceed 500 characters.");
    }
}
