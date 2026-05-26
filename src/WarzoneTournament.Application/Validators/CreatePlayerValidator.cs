using FluentValidation;
using WarzoneTournament.Application.DTOs.Player;

namespace WarzoneTournament.Application.Validators;

public class CreatePlayerValidator : AbstractValidator<CreatePlayerDto>
{
    public CreatePlayerValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MaximumLength(100).WithMessage("Username cannot exceed 100 characters.")
            .MinimumLength(2).WithMessage("Username must be at least 2 characters.")
            .Matches("^[A-Za-z0-9_\\-#]+$").WithMessage("Username can only contain letters, numbers, underscores, hyphens, and hash signs.");

        RuleFor(x => x.ActivisionId)
            .MaximumLength(100).WithMessage("Activision ID cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.ActivisionId));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.DiscordId)
            .Matches("^[0-9]{17,20}$").WithMessage("Discord ID must be a valid Discord snowflake ID.")
            .When(x => !string.IsNullOrEmpty(x.DiscordId));
    }
}
