using FluentValidation;
using WarzoneTournament.Application.DTOs.Match;

namespace WarzoneTournament.Application.Validators;

public class SubmitMatchResultsValidator : AbstractValidator<SubmitMatchResultsDto>
{
    public SubmitMatchResultsValidator()
    {
        RuleFor(x => x.TeamResults)
            .NotEmpty().WithMessage("At least one team result is required.")
            .Must(r => r.Select(t => t.TeamId).Distinct().Count() == r.Count)
            .WithMessage("Duplicate team entries are not allowed.");

        RuleForEach(x => x.TeamResults).SetValidator(new TeamResultValidator());
        RuleForEach(x => x.PlayerStats).SetValidator(new PlayerStatSubmitValidator());
    }
}

public class TeamResultValidator : AbstractValidator<TeamResultDto>
{
    public TeamResultValidator()
    {
        RuleFor(x => x.TeamId)
            .NotEmpty().WithMessage("Team ID is required.");

        RuleFor(x => x.Placement)
            .GreaterThan(0).WithMessage("Placement must be greater than 0.")
            .LessThanOrEqualTo(150).WithMessage("Placement cannot exceed 150.");

        RuleFor(x => x.Kills)
            .GreaterThanOrEqualTo(0).WithMessage("Kills cannot be negative.")
            .LessThanOrEqualTo(500).WithMessage("Kills seem unrealistic (max 500).");

        RuleFor(x => x.Deaths)
            .GreaterThanOrEqualTo(0).WithMessage("Deaths cannot be negative.");
    }
}

public class PlayerStatSubmitValidator : AbstractValidator<PlayerStatSubmitDto>
{
    public PlayerStatSubmitValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty().WithMessage("Player ID is required.");
        RuleFor(x => x.TeamId).NotEmpty().WithMessage("Team ID is required.");
        RuleFor(x => x.Kills).GreaterThanOrEqualTo(0).WithMessage("Kills cannot be negative.");
        RuleFor(x => x.Deaths).GreaterThanOrEqualTo(0).WithMessage("Deaths cannot be negative.");
        RuleFor(x => x.Damage).GreaterThanOrEqualTo(0).WithMessage("Damage cannot be negative.");
    }
}
