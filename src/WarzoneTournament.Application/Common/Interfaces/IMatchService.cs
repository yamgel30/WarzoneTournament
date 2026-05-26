using WarzoneTournament.Application.Common.Models;
using WarzoneTournament.Application.DTOs.Match;

namespace WarzoneTournament.Application.Common.Interfaces;

public interface IMatchService
{
    Task<Result<MatchDto>> CreateMatchAsync(CreateMatchDto dto, CancellationToken ct = default);
    Task<Result<MatchDto>> GetMatchByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<IReadOnlyList<MatchDto>>> GetMatchesByTournamentAsync(Guid tournamentId, CancellationToken ct = default);
    Task<Result<MatchDto>> UpdateMatchStatusAsync(Guid id, MatchStatus status, CancellationToken ct = default);
    Task<Result<MatchDto>> SubmitMatchResultsAsync(Guid id, SubmitMatchResultsDto dto, CancellationToken ct = default);
    Task<Result<MatchDto>> ConfirmMatchResultsAsync(Guid id, CancellationToken ct = default);
    Task<Result<IReadOnlyList<MatchTeamResultDto>>> GetMatchResultsAsync(Guid matchId, CancellationToken ct = default);
    Task<Result> DeleteMatchAsync(Guid id, CancellationToken ct = default);
}

public enum MatchStatus
{
    Pending = 0,
    CheckIn = 1,
    InProgress = 2,
    WaitingEvidence = 3,
    UnderReview = 4,
    Completed = 5,
    Disputed = 6,
    Cancelled = 7
}
