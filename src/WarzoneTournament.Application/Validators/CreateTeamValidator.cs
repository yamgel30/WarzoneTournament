using FluentValidation;
using WarzoneTournament.Application.DTOs.Team;

namespace WarzoneTournament.Application.Validators;

public class CreateTeamValidator : AbstractValidator<CreateTeamDto>
{
    public CreateTeamValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Team name is required.")
            .MaximumLength(100).WithMessage("Team name cannot exceed 100 characters.")
            .MinimumLength(2).WithMessage("Team name must be at least 2 characters.");

        RuleFor(x => x.Tag)
            .MaximumLength(10).WithMessage("Team tag cannot exceed 10 characters.")
            .Matches("^[A-Za-z0-9]+$").WithMessage("Team tag can only contain letters and numbers.")
            .When(x => !string.IsNullOrEmpty(x.Tag));

        RuleFor(x => x.ContactEmail)
            .EmailAddress().WithMessage("Contact email must be a valid email address.")
            .When(x => !string.IsNullOrEmpty(x.ContactEmail));
    }
}
