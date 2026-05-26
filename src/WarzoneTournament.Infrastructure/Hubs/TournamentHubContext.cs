namespace WarzoneTournament.Infrastructure.Hubs;

public interface ITournamentHubClient
{
    Task LeaderboardUpdated(Guid tournamentId, object leaderboard);
    Task MatchUpdated(Guid matchId, string status);
    Task EvidenceSubmitted(Guid matchId, Guid evidenceId);
    Task EvidenceReviewed(Guid evidenceId, string status);
    Task TournamentStatusChanged(Guid tournamentId, string status);
}
