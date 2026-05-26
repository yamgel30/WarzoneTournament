using FluentValidation;
using WarzoneTournament.Application.DTOs.Tournament;

namespace WarzoneTournament.Application.Validators;

public class CreateTournamentValidator : AbstractValidator<CreateTournamentDto>
{
    public CreateTournamentValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tournament name is required.")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.");

        RuleFor(x => x.MaxTeams)
            .GreaterThan(1).WithMessage("Maximum teams must be greater than 1.")
            .LessThanOrEqualTo(200).WithMessage("Maximum teams cannot exceed 200.");

        RuleFor(x => x.PlayersPerTeam)
            .GreaterThan(0).WithMessage("Players per team must be greater than 0.")
            .LessThanOrEqualTo(20).WithMessage("Players per team cannot exceed 20.");

        RuleFor(x => x.KillPoints)
            .GreaterThanOrEqualTo(0).WithMessage("Kill points cannot be negative.")
            .LessThanOrEqualTo(100).WithMessage("Kill points cannot exceed 100.");

        RuleFor(x => x.RegistrationEnd)
            .GreaterThan(x => x.RegistrationStart)
            .When(x => x.RegistrationStart.HasValue && x.RegistrationEnd.HasValue)
            .WithMessage("Registration end must be after registration start.");

        RuleFor(x => x.StartDate)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.StartDate.HasValue)
            .WithMessage("Start date must be in the future.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("End date must be after start date.");

        RuleFor(x => x.PrizePool)
            .GreaterThanOrEqualTo(0)
            .When(x => x.PrizePool.HasValue)
            .WithMessage("Prize pool cannot be negative.");

        RuleFor(x => x.StreamUrl)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrEmpty(x.StreamUrl))
            .WithMessage("Stream URL must be a valid URL.");
    }
}
