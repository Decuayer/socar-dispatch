using FluentValidation;

namespace SocarDispatch.Application.Features.Media.Commands.UploadMedia;

public class UploadMediaCommandValidator : AbstractValidator<UploadMediaCommand>
{
    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "video/mp4"
    };

    private const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50MB

    public UploadMediaCommandValidator()
    {
        RuleFor(x => x.File)
            .NotNull().WithMessage("A file to be uploaded must be selected.")
            .Must(file => file != null && file.Length > 0).WithMessage("The file to be uploaded cannot be empty.")
            .Must(file => file != null && file.Length <= MaxFileSizeBytes).WithMessage("The file size exceeds the 50MB limit.")
            .Must(file => file != null && AllowedMimeTypes.Contains(file.ContentType))
            .WithMessage("Unsupported file type. Only JPEG, PNG, and MP4 formats are accepted.");

        RuleFor(x => x.Category)
            .MaximumLength(50).WithMessage("The category name cannot exceed 50 characters.");
    }
}
