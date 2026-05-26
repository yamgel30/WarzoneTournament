namespace WarzoneTournament.Application.Common.Interfaces;

public interface IDiscordNotificationService
{
    Task SendMatchResultsAsync(Guid matchId, CancellationToken ct = default);
    Task SendLeaderboardUpdateAsync(Guid tournamentId, CancellationToken ct = default);
    Task NotifyTeamCheckInAsync(Guid teamId, Guid tournamentId, CancellationToken ct = default);
    Task SendEvidenceRejectionNotificationAsync(Guid evidenceId, string reason, CancellationToken ct = default);
    Task SendTournamentAnnouncementAsync(Guid tournamentId, string message, CancellationToken ct = default);
    Task StartBotAsync(CancellationToken ct = default);
    Task StopBotAsync(CancellationToken ct = default);
}
