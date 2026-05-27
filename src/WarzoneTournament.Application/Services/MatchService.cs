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
    private readonly IDiscordNotificationService _discord;

    public MatchService(IUnitOfWork uow, IMapper mapper, ILogger<MatchService> logger,
        ISignalRNotificationService signalR, ILeaderboardService leaderboard,
        IDiscordNotificationService discord)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
        _signalR = signalR;
        _leaderboard = leaderboard;
        _discord = discord;
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

        // Validate placements
        var teamCount  = dto.TeamResults.Count;
        var placements = dto.TeamResults.Select(r => r.Placement).ToList();
        if (placements.Any(p => p <= 0))
            return Result.Failure<MatchDto>("All teams must have a placement greater than 0.");
        if (placements.Any(p => p > teamCount))
            return Result.Failure<MatchDto>($"Placement cannot exceed the number of teams ({teamCount}).");
        var duplicates = placements.GroupBy(p => p).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicates.Any())
            return Result.Failure<MatchDto>($"Duplicate placement(s): {string.Join(", ", duplicates)}.");

        // Parse placement multipliers — formula: TotalPoints = Kills × Multiplier + BonusPoints
        var placementMultipliers = new Dictionary<int, double>();
        try
        {
            placementMultipliers = JsonSerializer.Deserialize<Dictionary<int, double>>(tournament.PlacementPointsJson)
                ?? new Dictionary<int, double>();
        }
        catch { }

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
                var multiplier = placementMultipliers.TryGetValue(teamResult.Placement, out var m) ? m : 1.0;
                var killPts = Math.Round(teamResult.Kills * multiplier, 2);
                var totalPts = killPts + teamResult.BonusPoints;

                await _uow.MatchTeamResults.AddAsync(new MatchTeamResult
                {
                    MatchId = id,
                    TeamId = teamResult.TeamId,
                    Placement = teamResult.Placement,
                    Kills = teamResult.Kills,
                    Deaths = teamResult.Deaths,
                    PlacementMultiplier = multiplier,
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

            // Capture Match Point teams BEFORE recalculating so we can detect a victory
            var preMatchMPIds = (await _uow.TournamentTeams.FindAsync(
                tt => tt.TournamentId == match.TournamentId && tt.IsMatchPoint, ct))
                .Select(tt => tt.TeamId).ToHashSet();

            // Update tournament leaderboard (also sets newly-reached IsMatchPoint flags)
            await _leaderboard.RecalculateLeaderboardAsync(match.TournamentId, ct);
            await _signalR.NotifyLeaderboardUpdatedAsync(match.TournamentId, ct);
            await _discord.SendMatchResultsAsync(id, ct);
            await _discord.SendLeaderboardUpdateAsync(match.TournamentId, ct);

            // Notify teams that newly entered Match Point this match
            var newlyMP = (await _uow.TournamentTeams.FindAsync(
                tt => tt.TournamentId == match.TournamentId && tt.IsMatchPoint, ct))
                .Where(tt => !preMatchMPIds.Contains(tt.TeamId)).ToList();
            foreach (var mpTeam in newlyMP)
            {
                var team = await _uow.Teams.GetByIdAsync(mpTeam.TeamId, ct);
                await _discord.SendTournamentAnnouncementAsync(match.TournamentId,
                    $"⚠️ ¡**{team?.Name}** está en **Match Point**! Solo necesitan ganar 1 partida más para ser campeones.", ct);
            }

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
        await _discord.SendMatchResultsAsync(id, ct);
        await _discord.SendLeaderboardUpdateAsync(match.TournamentId, ct);

        // Check for Match Point victory on confirm
        var mpTeamIds = (await _uow.TournamentTeams.FindAsync(
            tt => tt.TournamentId == match.TournamentId && tt.IsMatchPoint, ct))
            .Select(tt => tt.TeamId).ToHashSet();

        var firstPlaceResult = results.FirstOrDefault(r => r.Placement == 1);
        if (firstPlaceResult != null && mpTeamIds.Contains(firstPlaceResult.TeamId))
        {
            var t = await _uow.Tournaments.GetByIdAsync(match.TournamentId, ct);
            if (t is { Status: not Domain.Enums.TournamentStatus.Completed })
            {
                t.Status = Domain.Enums.TournamentStatus.Completed;
                t.WinnerTeamId = firstPlaceResult.TeamId;
                t.EndDate ??= DateTime.UtcNow;
                _uow.Tournaments.Update(t);
                await _uow.SaveChangesAsync(ct);
                var winnerTeam = await _uow.Teams.GetByIdAsync(firstPlaceResult.TeamId, ct);
                await _discord.SendTournamentAnnouncementAsync(match.TournamentId,
                    $"🏆 ¡**{winnerTeam?.Name ?? "Un equipo"}** gana el torneo con Match Point!", ct);
                await _signalR.NotifyTournamentStatusChangedAsync(match.TournamentId, "Completed", ct);
            }
        }

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
                PlacementMultiplier = r.PlacementMultiplier,
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
        var playerStats = await _uow.PlayerMatchStats.FindAsync(s => s.MatchId == match.Id, ct);
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
            PlayerStats = playerStats.Select(s => new MatchPlayerStatDto
            {
                PlayerId = s.PlayerId,
                TeamId   = s.TeamId,
                Kills    = s.Kills
            }).ToList(),
            EvidenceCount = evidenceCount,
            CreatedAt = match.CreatedAt
        };
    }
}
