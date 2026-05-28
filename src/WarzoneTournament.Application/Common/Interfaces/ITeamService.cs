using WarzoneTournament.Application.Common.Models;
using WarzoneTournament.Application.DTOs.Team;

namespace WarzoneTournament.Application.Common.Interfaces;

public interface ITeamService
{
    Task<Result<TeamDto>> CreateTeamAsync(CreateTeamDto dto, CancellationToken ct = default);
    Task<Result<TeamDto>> GetTeamByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<IReadOnlyList<TeamDto>>> GetTeamsAsync(CancellationToken ct = default);
    Task<Result<TeamDto>> UpdateTeamAsync(Guid id, UpdateTeamDto dto, CancellationToken ct = default);
    Task<Result> RegisterTeamForTournamentAsync(Guid teamId, Guid tournamentId, CancellationToken ct = default);
    Task<Result> CheckInTeamAsync(Guid teamId, Guid tournamentId, CancellationToken ct = default);
    Task<Result> AddPlayerToTeamAsync(Guid teamId, Guid playerId, CancellationToken ct = default);
    Task<Result> RemovePlayerFromTeamAsync(Guid teamId, Guid playerId, CancellationToken ct = default);
    Task<Result> SetCaptainAsync(Guid teamId, Guid? captainPlayerId, CancellationToken ct = default);
    Task<Result> DeleteTeamAsync(Guid id, CancellationToken ct = default);
    Task<Result<IReadOnlyList<TournamentTeamStatusDto>>> GetTeamsWithTournamentStatusAsync(Guid tournamentId, CancellationToken ct = default);
    Task<Result> UnregisterTeamFromTournamentAsync(Guid teamId, Guid tournamentId, CancellationToken ct = default);
}
