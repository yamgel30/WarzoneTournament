using Microsoft.Extensions.Logging;
using WarzoneTournament.Application.Common.Interfaces;
using WarzoneTournament.Application.Common.Models;
using WarzoneTournament.Application.DTOs.Match;
using WarzoneTournament.Application.DTOs.Round;
using WarzoneTournament.Domain.Entities;
using WarzoneTournament.Domain.Interfaces;

namespace WarzoneTournament.Application.Services;

public class RoundService : IRoundService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<RoundService> _logger;

    public RoundService(IUnitOfWork uow, ILogger<RoundService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result<RoundDto>> CreateRoundAsync(CreateRoundDto dto, CancellationToken ct = default)
    {
        var tournament = await _uow.Tournaments.GetByIdAsync(dto.TournamentId, ct);
        if (tournament is null) return Result.Failure<RoundDto>("Tournament not found.");

        var exists = await _uow.Rounds.ExistsAsync(
            r => r.TournamentId == dto.TournamentId && r.RoundNumber == dto.RoundNumber, ct);
        if (exists) return Result.Failure<RoundDto>($"Round number {dto.RoundNumber} already exists in this tournament.");

        var round = new Round
        {
            TournamentId = dto.TournamentId,
            RoundNumber = dto.RoundNumber,
            Name = dto.Name,
            StartTime = dto.StartTime,
            Notes = dto.Notes
        };

        await _uow.Rounds.AddAsync(round, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Round {Number} created for tournament {TournamentId}", dto.RoundNumber, dto.TournamentId);
        return Result.Success(ToDto(round, new List<MatchDto>()));
    }

    public async Task<Result<IReadOnlyList<RoundDto>>> GetRoundsByTournamentAsync(Guid tournamentId, CancellationToken ct = default)
    {
        var rounds = await _uow.Rounds.FindAsync(r => r.TournamentId == tournamentId, ct);
        var dtos = new List<RoundDto>();

        foreach (var round in rounds.OrderBy(r => r.RoundNumber))
        {
            var matches = await _uow.Matches.FindAsync(m => m.RoundId == round.Id, ct);
            var matchDtos = matches.OrderBy(m => m.MatchNumber).Select(m => new MatchDto
            {
                Id = m.Id,
                RoundId = m.RoundId,
                RoundName = round.Name,
                TournamentId = m.TournamentId,
                MatchNumber = m.MatchNumber,
                Status = (Domain.Enums.MatchStatus)m.Status,
                ScheduledTime = m.ScheduledTime,
                StartTime = m.StartTime,
                EndTime = m.EndTime,
                LobbyCode = m.LobbyCode,
                LobbyPassword = m.LobbyPassword,
                MapName = m.MapName,
                Notes = m.Notes,
                ResultsConfirmed = m.ResultsConfirmed,
                CreatedAt = m.CreatedAt
            }).ToList();

            dtos.Add(ToDto(round, matchDtos));
        }

        return Result.Success<IReadOnlyList<RoundDto>>(dtos);
    }

    public async Task<Result> DeleteRoundAsync(Guid id, CancellationToken ct = default)
    {
        var round = await _uow.Rounds.GetByIdAsync(id, ct);
        if (round is null) return Result.Failure("Round not found.");

        var hasMatches = await _uow.Matches.ExistsAsync(m => m.RoundId == id, ct);
        if (hasMatches) return Result.Failure("Cannot delete a round that has matches. Delete the matches first.");

        _uow.Rounds.Remove(round);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    private static RoundDto ToDto(Round round, List<MatchDto> matches) => new()
    {
        Id = round.Id,
        TournamentId = round.TournamentId,
        RoundNumber = round.RoundNumber,
        Name = round.Name,
        StartTime = round.StartTime,
        EndTime = round.EndTime,
        IsCompleted = round.IsCompleted,
        Notes = round.Notes,
        Matches = matches
    };
}
