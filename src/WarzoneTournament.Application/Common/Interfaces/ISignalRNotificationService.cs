namespace WarzoneTournament.Application.Common.Interfaces;

public interface ISignalRNotificationService
{
    Task NotifyLeaderboardUpdatedAsync(Guid tournamentId, CancellationToken ct = default);
    Task NotifyMatchUpdatedAsync(Guid matchId, string status, CancellationToken ct = default);
    Task NotifyEvidenceSubmittedAsync(Guid matchId, Guid evidenceId, CancellationToken ct = default);
    Task NotifyEvidenceReviewedAsync(Guid evidenceId, string status, CancellationToken ct = default);
    Task NotifyTournamentStatusChangedAsync(Guid tournamentId, string status, CancellationToken ct = default);
}
