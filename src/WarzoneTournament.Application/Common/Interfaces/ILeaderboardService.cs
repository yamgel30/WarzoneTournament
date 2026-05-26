using WarzoneTournament.Application.Common.Models;
using WarzoneTournament.Application.DTOs.Leaderboard;

namespace WarzoneTournament.Application.Common.Interfaces;

public interface ILeaderboardService
{
    Task<Result<IReadOnlyList<LeaderboardEntryDto>>> GetTournamentLeaderboardAsync(Guid tournamentId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<LeaderboardEntryDto>>> GetRoundLeaderboardAsync(Guid tournamentId, Guid roundId, CancellationToken ct = default);
    Task<Result> RecalculateLeaderboardAsync(Guid tournamentId, CancellationToken ct = default);
}
