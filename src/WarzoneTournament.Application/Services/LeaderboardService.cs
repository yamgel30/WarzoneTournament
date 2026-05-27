using AutoMapper;
using Microsoft.Extensions.Logging;
using WarzoneTournament.Application.Common.Interfaces;
using WarzoneTournament.Application.Common.Models;
using WarzoneTournament.Application.DTOs.Leaderboard;
using WarzoneTournament.Domain.Interfaces;

namespace WarzoneTournament.Application.Services;

public class LeaderboardService : ILeaderboardService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<LeaderboardService> _logger;

    public LeaderboardService(IUnitOfWork uow, ILogger<LeaderboardService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<LeaderboardEntryDto>>> GetTournamentLeaderboardAsync(
        Guid tournamentId, CancellationToken ct = default)
    {
        var tournamentTeams = await _uow.TournamentTeams.FindAsNoTrackingAsync(tt => tt.TournamentId == tournamentId, ct);
        var matches = await _uow.Matches.FindAsNoTrackingAsync(m => m.TournamentId == tournamentId, ct);
        var matchIds = matches.Select(m => m.Id).ToHashSet();

        var entries = new List<LeaderboardEntryDto>();

        foreach (var tt in tournamentTeams)
        {
            var team = await _uow.Teams.GetByIdAsync(tt.TeamId, ct);
            if (team is null) continue;

            var teamResults = await _uow.MatchTeamResults.FindAsNoTrackingAsync(
                r => r.TeamId == tt.TeamId && matchIds.Contains(r.MatchId), ct);

            var matchScores = new List<MatchScoreDto>();
            foreach (var result in teamResults.OrderBy(r => r.CreatedAt))
            {
                var match = matches.FirstOrDefault(m => m.Id == result.MatchId);
                matchScores.Add(new MatchScoreDto
                {
                    MatchId = result.MatchId,
                    MatchNumber = match?.MatchNumber ?? 0,
                    Placement = result.Placement,
                    Kills = result.Kills,
                    Points = result.TotalPoints
                });
            }

            entries.Add(new LeaderboardEntryDto
            {
                TeamId = tt.TeamId,
                TeamName = team.Name,
                TeamTag = team.Tag,
                TeamLogoUrl = team.LogoUrl,
                TotalPoints = teamResults.Sum(r => r.TotalPoints),
                TotalKills = teamResults.Sum(r => r.Kills),
                TotalKillPoints = teamResults.Sum(r => r.KillPoints),
                TotalBonusPoints = teamResults.Sum(r => r.BonusPoints),
                MatchesPlayed = teamResults.Count(),
                BestPlacement = teamResults.Any() ? teamResults.Min(r => r.Placement) : 0,
                CheckedIn = tt.CheckedIn,
                IsEliminated = tt.IsEliminated,
                IsMatchPoint = tt.IsMatchPoint,
                MatchScores = matchScores
            });
        }

        // Sort: total points desc, then kills desc
        var ranked = entries
            .OrderByDescending(e => e.TotalPoints)
            .ThenByDescending(e => e.TotalKills)
            .ThenBy(e => e.BestPlacement)
            .ToList();

        for (int i = 0; i < ranked.Count; i++)
            ranked[i].Rank = i + 1;

        return Result.Success<IReadOnlyList<LeaderboardEntryDto>>(ranked);
    }

    public async Task<Result<IReadOnlyList<LeaderboardEntryDto>>> GetRoundLeaderboardAsync(
        Guid tournamentId, Guid roundId, CancellationToken ct = default)
    {
        var matches = await _uow.Matches.FindAsync(m => m.TournamentId == tournamentId && m.RoundId == roundId, ct);
        var matchIds = matches.Select(m => m.Id).ToHashSet();

        var tournamentTeams = await _uow.TournamentTeams.FindAsync(tt => tt.TournamentId == tournamentId, ct);
        var entries = new List<LeaderboardEntryDto>();

        foreach (var tt in tournamentTeams)
        {
            var team = await _uow.Teams.GetByIdAsync(tt.TeamId, ct);
            if (team is null) continue;

            var teamResults = await _uow.MatchTeamResults.FindAsync(
                r => r.TeamId == tt.TeamId && matchIds.Contains(r.MatchId), ct);

            if (!teamResults.Any()) continue;

            entries.Add(new LeaderboardEntryDto
            {
                TeamId = tt.TeamId,
                TeamName = team.Name,
                TeamTag = team.Tag,
                TeamLogoUrl = team.LogoUrl,
                TotalPoints = teamResults.Sum(r => r.TotalPoints),
                TotalKills = teamResults.Sum(r => r.Kills),
                TotalKillPoints = teamResults.Sum(r => r.KillPoints),
                TotalBonusPoints = teamResults.Sum(r => r.BonusPoints),
                MatchesPlayed = teamResults.Count(),
                BestPlacement = teamResults.Min(r => r.Placement),
                CheckedIn = tt.CheckedIn,
                IsEliminated = tt.IsEliminated,
                IsMatchPoint = tt.IsMatchPoint
            });
        }

        var ranked = entries
            .OrderByDescending(e => e.TotalPoints)
            .ThenByDescending(e => e.TotalKills)
            .ToList();

        for (int i = 0; i < ranked.Count; i++)
            ranked[i].Rank = i + 1;

        return Result.Success<IReadOnlyList<LeaderboardEntryDto>>(ranked);
    }

    public async Task<Result<IReadOnlyList<PlayerLeaderboardEntryDto>>> GetPlayerLeaderboardAsync(
        Guid tournamentId, CancellationToken ct = default)
    {
        var matches = await _uow.Matches.FindAsNoTrackingAsync(m => m.TournamentId == tournamentId, ct);
        var matchIds = matches.Select(m => m.Id).ToHashSet();

        var allStats = await _uow.PlayerMatchStats.FindAsNoTrackingAsync(s => matchIds.Contains(s.MatchId), ct);

        var grouped = allStats
            .GroupBy(s => s.PlayerId)
            .Select(g => new { PlayerId = g.Key, TeamId = g.First().TeamId, TotalKills = g.Sum(x => x.Kills), MatchesPlayed = g.Select(x => x.MatchId).Distinct().Count() })
            .OrderByDescending(x => x.TotalKills)
            .ToList();

        var result = new List<PlayerLeaderboardEntryDto>();
        int rank = 1;
        foreach (var g in grouped)
        {
            var player = await _uow.Players.GetByIdAsync(g.PlayerId, ct);
            var team = await _uow.Teams.GetByIdAsync(g.TeamId, ct);
            result.Add(new PlayerLeaderboardEntryDto
            {
                Rank         = rank++,
                PlayerId     = g.PlayerId,
                Username     = player?.Username ?? "Unknown",
                TeamName     = team?.Name,
                TeamTag      = team?.Tag,
                TotalKills   = g.TotalKills,
                MatchesPlayed = g.MatchesPlayed
            });
        }

        return Result.Success<IReadOnlyList<PlayerLeaderboardEntryDto>>(result);
    }

    public async Task<Result> RecalculateLeaderboardAsync(Guid tournamentId, CancellationToken ct = default)
    {
        var leaderboardResult = await GetTournamentLeaderboardAsync(tournamentId, ct);
        if (leaderboardResult.IsFailure)
            return Result.Failure(leaderboardResult.Error!);

        var tournament = await _uow.Tournaments.GetByIdAsync(tournamentId, ct);
        var tournamentTeams = await _uow.TournamentTeams.FindAsync(tt => tt.TournamentId == tournamentId, ct);

        foreach (var entry in leaderboardResult.Value)
        {
            var tt = tournamentTeams.FirstOrDefault(t => t.TeamId == entry.TeamId);
            if (tt is null) continue;

            tt.TotalPoints = entry.TotalPoints;
            tt.TotalKills = entry.TotalKills;
            tt.CurrentRank = entry.Rank;

            // Once a team reaches the threshold it stays in Match Point
            if (tournament?.MatchPointThreshold.HasValue == true
                && entry.TotalPoints >= tournament.MatchPointThreshold.Value)
                tt.IsMatchPoint = true;

            _uow.TournamentTeams.Update(tt);
        }

        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Leaderboard recalculated for tournament {TournamentId}", tournamentId);
        return Result.Success();
    }
}
