using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using WarzoneTournament.Application.Common.Interfaces;
using WarzoneTournament.Application.DTOs.Leaderboard;

namespace WarzoneTournament.Infrastructure.Services;

public class SignalRNotificationService : ISignalRNotificationService
{
    private readonly IHubContext<Microsoft.AspNetCore.SignalR.Hub> _hubContext;
    private readonly ILogger<SignalRNotificationService> _logger;
    private readonly ILeaderboardService _leaderboard;

    public SignalRNotificationService(
        IHubContext<Microsoft.AspNetCore.SignalR.Hub> hubContext,
        ILogger<SignalRNotificationService> logger,
        ILeaderboardService leaderboard)
    {
        _hubContext = hubContext;
        _logger = logger;
        _leaderboard = leaderboard;
    }

    public async Task NotifyLeaderboardUpdatedAsync(Guid tournamentId, CancellationToken ct = default)
    {
        try
        {
            var leaderboardResult = await _leaderboard.GetTournamentLeaderboardAsync(tournamentId, ct);
            if (leaderboardResult.IsSuccess)
            {
                await _hubContext.Clients.Group($"tournament-{tournamentId}")
                    .SendAsync("LeaderboardUpdated", tournamentId, leaderboardResult.Value, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify leaderboard update for tournament {Id}", tournamentId);
        }
    }

    public async Task NotifyMatchUpdatedAsync(Guid matchId, string status, CancellationToken ct = default)
    {
        try
        {
            await _hubContext.Clients.Group($"match-{matchId}")
                .SendAsync("MatchUpdated", matchId, status, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify match update for match {Id}", matchId);
        }
    }

    public async Task NotifyEvidenceSubmittedAsync(Guid matchId, Guid evidenceId, CancellationToken ct = default)
    {
        try
        {
            await _hubContext.Clients.Group($"match-{matchId}")
                .SendAsync("EvidenceSubmitted", matchId, evidenceId, ct);
            await _hubContext.Clients.Group("admins")
                .SendAsync("EvidenceSubmitted", matchId, evidenceId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify evidence submitted");
        }
    }

    public async Task NotifyEvidenceReviewedAsync(Guid evidenceId, string status, CancellationToken ct = default)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("EvidenceReviewed", evidenceId, status, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify evidence reviewed for {Id}", evidenceId);
        }
    }

    public async Task NotifyTournamentStatusChangedAsync(Guid tournamentId, string status, CancellationToken ct = default)
    {
        try
        {
            await _hubContext.Clients.Group($"tournament-{tournamentId}")
                .SendAsync("TournamentStatusChanged", tournamentId, status, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify tournament status changed for {Id}", tournamentId);
        }
    }
}
