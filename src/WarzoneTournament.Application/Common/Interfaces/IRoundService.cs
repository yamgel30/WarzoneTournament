using WarzoneTournament.Application.Common.Models;
using WarzoneTournament.Application.DTOs.Round;

namespace WarzoneTournament.Application.Common.Interfaces;

public interface IRoundService
{
    Task<Result<RoundDto>> CreateRoundAsync(CreateRoundDto dto, CancellationToken ct = default);
    Task<Result<IReadOnlyList<RoundDto>>> GetRoundsByTournamentAsync(Guid tournamentId, CancellationToken ct = default);
    Task<Result> DeleteRoundAsync(Guid id, CancellationToken ct = default);
}
