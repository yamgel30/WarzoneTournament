using WarzoneTournament.Application.Common.Models;
using WarzoneTournament.Application.DTOs.Tournament;

namespace WarzoneTournament.Application.Common.Interfaces;

public interface ITournamentService
{
    Task<Result<TournamentDto>> CreateTournamentAsync(CreateTournamentDto dto, CancellationToken ct = default);
    Task<Result<TournamentDto>> UpdateTournamentAsync(Guid id, UpdateTournamentDto dto, CancellationToken ct = default);
    Task<Result<TournamentDto>> GetTournamentByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<PagedResult<TournamentListDto>>> GetTournamentsAsync(TournamentQueryDto query, CancellationToken ct = default);
    Task<Result> DeleteTournamentAsync(Guid id, CancellationToken ct = default);
    Task<Result<TournamentDto>> StartCheckInAsync(Guid id, CancellationToken ct = default);
    Task<Result<TournamentDto>> StartTournamentAsync(Guid id, CancellationToken ct = default);
    Task<Result<TournamentDto>> CompleteTournamentAsync(Guid id, CancellationToken ct = default);
    Task<Result<TournamentDto>> CancelTournamentAsync(Guid id, CancellationToken ct = default);
}
