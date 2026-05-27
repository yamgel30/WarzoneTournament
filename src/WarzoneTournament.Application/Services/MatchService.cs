using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using WarzoneTournament.Application.Common.Interfaces;
using WarzoneTournament.Application.Common.Models;
using WarzoneTournament.Application.DTOs.Match;
using WarzoneTournament.Domain.Entities;
using WarzoneTournament.Domain.Enums;
using WarzoneTournament.Domain.Interfaces;
using AppMatchStatus = WarzoneTournament.Application.Common.Interfaces.MatchStatus;
using DomainMatchStatus = WarzoneTournament.Domain.Enums.MatchStatus;

namespace WarzoneTournament.Application.Services;

public class MatchService : IMatchService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<MatchService> _logger;
    private readonly ISignalRNotificationService _signalR;
    private readonly ILeaderboardService _leaderboard;

    public MatchService(IUnitOfWork uow, IMapper mapper, ILogger<MatchService> logger,
        ISignalRNotificationService signalR, ILeaderboardService leaderboard)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
        _signalR = signalR;
        _leaderboard = leaderboard;
    }

    public async Task<Result<MatchDto>> CreateMatchAsync(CreateMatchDto dto, CancellationToken ct = default)
    {
        var round = await _uow.Rounds.GetByIdAsync(dto.RoundId, ct);
        if (round is null) return Result.Failure<MatchDto>("Round not found.");

        var tournament = await _uow.Tournaments.GetByIdAsync(dto.TournamentId, ct);
        if (tournament is null) return Result.Failure<MatchDto>("Tournament not found.");

        var match = new Match
        {
            RoundId = dto.RoundId,
            TournamentId = dto.TournamentId,
            MatchNumber = dto.MatchNumber,
            ScheduledTime = dto.ScheduledTime,
            LobbyCode = dto.LobbyCode,
            LobbyPassword = dto.LobbyPassword,
            MapName = dto.MapName,
            Notes = dto.Notes
        };

        await _uow.Matches.AddAsync(match, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Match {Number} created for tournament {TournamentId}", dto.MatchNumber, dto.TournamentId);
        return Result.Success(await BuildMatchDtoAsync(match, ct));
    }

    public async Task<Result<MatchDto>> GetMatchByIdAsync(Guid id, CancellationToken ct = default)
    {
        var match = await _uow.Matches.GetByIdAsync(id, ct);
        if (match is null) return Result.Failure<MatchDto>("Match not found.");

        return Result.Success(await BuildMatchDtoAsync(match, ct));
    }

    public async Task<Result<IReadOnlyList<MatchDto>>> GetMatchesByTournamentAsync(Guid tournamentId, CancellationToken ct = default)
    {
        var matches = await _uow.Matches.FindAsync(m => m.TournamentId == tournamentId, ct);
        var dtos = new List<MatchDto>();
        foreach (var match in matches.OrderBy(m => m.MatchNumber))
            dtos.Add(await BuildMatchDtoAsync(match, ct));

        return Result.Success<IReadOnlyList<MatchDto>>(dtos);
    }

    public async Task<Result<MatchDto>> UpdateMatchStatusAsync(Guid id, AppMatchStatus status, CancellationToken ct = default)
    {
        var match = await _uow.Matches.GetByIdAsync(id, ct);
        if (match is null) return Result.Failure<MatchDto>("Match not found.");

        var domainStatus = (DomainMatchStatus)(int)status;
        match.Status = domainStatus;
        if (domainStatus == DomainMatchStatus.InProgress && match.StartTime is null)
            match.StartTime = DateTime.UtcNow;
        if (domainStatus == DomainMatchStatus.Completed && match.EndTime is null)
            match.EndTime = DateTime.UtcNow;

        _uow.Matches.Update(match);
        await _uow.SaveChangesAsync(ct);
        await _signalR.NotifyMatchUpdatedAsync(id, domainStatus.ToString(), ct);

        return Result.Success(await BuildMatchDtoAsync(match, ct));
    }

    public async Task<Result<MatchDto>> SubmitMatchResultsAsync(Guid id, SubmitMatchResultsDto dto, CancellationToken ct = default)
    {
        var match = await _uow.Matches.GetByIdAsync(id, ct);
        if (match is null) return Result.Failure<MatchDto>("Match not found.");

        var tournament = await _uow.Tournaments.GetByIdAsync(match.TournamentId, ct);
        if (tournament is null) return Result.Failure<MatchDto>("Tournament not found.");

        // Parse placement points configuration
        var placementPoints = new Dictionary<int, int>();
        try
        {
            placementPoints = JsonSerializer.Deserialize<Dictionary<int, int>>(tournament.PlacementPointsJson)
                ?? new Dictionary<int, int>();
        }
        catch
        {
            // Use default if parsing fails
        }

        await _uow.BeginTransactionAsync(ct);
        try
        {
            // Hard-delete existing results so the unique index (MatchId, TeamId) doesn't block re-insertion
            var existingResults = await _uow.MatchTeamResults.FindIncludingDeletedAsync(r => r.MatchId == id, ct);
            _uow.MatchTeamResults.HardRemoveRange(existingResults);

            var existingStats = await _uow.PlayerMatchStats.FindIncludingDeletedAsync(s => s.MatchId == id, ct);
            _uow.PlayerMatchStats.HardRemoveRange(existingStats);

            await _uow.SaveChangesAsync(ct);

            // Insert team results with calculated points
            foreach (var teamResult in dto.TeamResults)
            {
                placementPoints.TryGetValue(teamResult.Placement, out int placePts);
                var killPts = teamResult.Kills * tournament.KillPoints;
                var totalPts = placePts + killPts + teamResult.BonusPoints;

                await _uow.MatchTeamResults.AddAsync(new MatchTeamResult
                {
                    MatchId = id,
                    TeamId = teamResult.TeamId,
                    Placement = teamResult.Placement,
                    Kills = teamResult.Kills,
                    Deaths = teamResult.Deaths,
                    PlacementPoints = placePts,
                    KillPoints = killPts,
                    BonusPoints = teamResult.BonusPoints,
                    TotalPoints = totalPts
                }, ct);
            }

            // Insert player stats — deduplicate by PlayerId (a player can only appear once per match)
            var dedupedPlayerStats = dto.PlayerStats
                .GroupBy(p => p.PlayerId)
                .Select(g => g.First())
                .ToList();

            foreach (var playerStat in dedupedPlayerStats)
            {
                await _uow.PlayerMatchStats.AddAsync(new PlayerMatchStats
                {
                    MatchId = id,
                    PlayerId = playerStat.PlayerId,
                    TeamId = playerStat.TeamId,
                    Kills = playerStat.Kills,
                    Deaths = playerStat.Deaths,
                    Assists = playerStat.Assists,
                    Damage = playerStat.Damage,
                    HeadshotPercentage = playerStat.HeadshotPercentage,
                    GulagWins = playerStat.GulagWins,
                    GulagAttempts = playerStat.GulagAttempts,
                    Revives = playerStat.Revives,
                    DistanceTraveled = playerStat.DistanceTraveled
                }, ct);
            }

            match.Status = DomainMatchStatus.WaitingEvidence;
            _uow.Matches.Update(match);
            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);

            // Update tournament leaderboard
            await _leaderboard.RecalculateLeaderboardAsync(match.TournamentId, ct);
            await _signalR.NotifyLeaderboardUpdatedAsync(match.TournamentId, ct);

            _logger.LogInformation("Results submitted for match {MatchId}", id);
            return Result.Success(await BuildMatchDtoAsync(match, ct));
        }
        catch (Exception ex)
        {
            await _uow.RollbackTransactionAsync(ct);
            _logger.LogError(ex, "Error submitting results for match {MatchId}", id);
            return Result.Failure<MatchDto>("Failed to submit match results.");
        }
    }

    public async Task<Result<MatchDto>> ConfirmMatchResultsAsync(Guid id, CancellationToken ct = default)
    {
        var match = await _uow.Matches.GetByIdAsync(id, ct);
        if (match is null) return Result.Failure<MatchDto>("Match not found.");

        match.Status = DomainMatchStatus.Completed;
        match.ResultsConfirmed = true;
        match.EndTime ??= DateTime.UtcNow;
        _uow.Matches.Update(match);

        // Mark team results as verified
        var results = await _uow.MatchTeamResults.FindAsync(r => r.MatchId == id, ct);
        foreach (var result in results)
        {
            result.IsVerified = true;
            _uow.MatchTeamResults.Update(result);
        }

        await _uow.SaveChangesAsync(ct);
        await _signalR.NotifyMatchUpdatedAsync(id, "Completed", ct);
        await _leaderboard.RecalculateLeaderboardAsync(match.TournamentId, ct);
        await _signalR.NotifyLeaderboardUpdatedAsync(match.TournamentId, ct);

        return Result.Success(await BuildMatchDtoAsync(match, ct));
    }

    public async Task<Result<IReadOnlyList<MatchTeamResultDto>>> GetMatchResultsAsync(Guid matchId, CancellationToken ct = default)
    {
        var results = await _uow.MatchTeamResults.FindAsync(r => r.MatchId == matchId, ct);
        var dtos = new List<MatchTeamResultDto>();
        foreach (var r in results.OrderBy(x => x.Placement))
        {
            var team = await _uow.Teams.GetByIdAsync(r.TeamId, ct);
            dtos.Add(new MatchTeamResultDto
            {
                Id = r.Id,
                TeamId = r.TeamId,
                TeamName = team?.Name ?? "Unknown",
                TeamTag = team?.Tag,
                TeamLogoUrl = team?.LogoUrl,
                Placement = r.Placement,
                Kills = r.Kills,
                Deaths = r.Deaths,
                PlacementPoints = r.PlacementPoints,
                KillPoints = r.KillPoints,
                BonusPoints = r.BonusPoints,
                TotalPoints = r.TotalPoints,
                IsVerified = r.IsVerified
            });
        }
        return Result.Success<IReadOnlyList<MatchTeamResultDto>>(dtos);
    }

    public async Task<Result> DeleteMatchAsync(Guid id, CancellationToken ct = default)
    {
        var match = await _uow.Matches.GetByIdAsync(id, ct);
        if (match is null) return Result.Failure("Match not found.");

        if (match.Status == DomainMatchStatus.Completed)
            return Result.Failure("Cannot delete a completed match.");

        _uow.Matches.Remove(match);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task<MatchDto> BuildMatchDtoAsync(Match match, CancellationToken ct)
    {
        var round = await _uow.Rounds.GetByIdAsync(match.RoundId, ct);
        var tournament = await _uow.Tournaments.GetByIdAsync(match.TournamentId, ct);
        var results = await GetMatchResultsAsync(match.Id, ct);
        var evidenceCount = await _uow.MatchEvidences.CountAsync(e => e.MatchId == match.Id, ct);

        return new MatchDto
        {
            Id = match.Id,
            RoundId = match.RoundId,
            RoundName = round?.Name,
            TournamentId = match.TournamentId,
            TournamentName = tournament?.Name,
            MatchNumber = match.MatchNumber,
            Status = (Domain.Enums.MatchStatus)match.Status,
            ScheduledTime = match.ScheduledTime,
            StartTime = match.StartTime,
            EndTime = match.EndTime,
            LobbyCode = match.LobbyCode,
            LobbyPassword = match.LobbyPassword,
            MapName = match.MapName,
            Notes = match.Notes,
            ResultsConfirmed = match.ResultsConfirmed,
            TeamResults = results.IsSuccess ? results.Value.ToList() : new(),
            EvidenceCount = evidenceCount,
            CreatedAt = match.CreatedAt
        };
    }
}
