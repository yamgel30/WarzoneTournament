using FluentValidation;
using WarzoneTournament.Application.DTOs.Evidence;

namespace WarzoneTournament.Application.Validators;

public class SubmitEvidenceValidator : AbstractValidator<SubmitEvidenceDto>
{
    private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/gif", "image/webp" };
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB

    public SubmitEvidenceValidator()
    {
        RuleFor(x => x.MatchId)
            .NotEmpty().WithMessage("Match ID is required.");

        RuleFor(x => x.SubmittedByTeamId)
            .NotEmpty().WithMessage("Team ID is required.");

        RuleFor(x => x.ImageUrl)
            .NotEmpty().WithMessage("Image URL is required.")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Image URL must be a valid URL.");

        RuleFor(x => x.FileSizeBytes)
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .When(x => x.FileSizeBytes.HasValue)
            .WithMessage($"File size cannot exceed {MaxFileSizeBytes / 1024 / 1024}MB.");

        RuleFor(x => x.MimeType)
            .Must(mime => AllowedMimeTypes.Contains(mime!.ToLower()))
            .When(x => !string.IsNullOrEmpty(x.MimeType))
            .WithMessage("Only image files are allowed (JPEG, PNG, GIF, WEBP).");
    }
}
