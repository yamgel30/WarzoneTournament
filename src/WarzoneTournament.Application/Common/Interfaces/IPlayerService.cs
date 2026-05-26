using WarzoneTournament.Application.Common.Models;
using WarzoneTournament.Application.DTOs.Player;

namespace WarzoneTournament.Application.Common.Interfaces;

public interface IPlayerService
{
    Task<Result<PlayerDto>> CreatePlayerAsync(CreatePlayerDto dto, CancellationToken ct = default);
    Task<Result<PlayerDto>> GetPlayerByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<PlayerDto>> GetPlayerByDiscordIdAsync(string discordId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<PlayerDto>>> GetPlayersAsync(CancellationToken ct = default);
    Task<Result<PlayerDto>> UpdatePlayerAsync(Guid id, UpdatePlayerDto dto, CancellationToken ct = default);
    Task<Result<PlayerStatsDto>> GetPlayerStatsAsync(Guid playerId, Guid? tournamentId = null, CancellationToken ct = default);
    Task<Result> BanPlayerAsync(Guid playerId, string reason, CancellationToken ct = default);
    Task<Result> UnbanPlayerAsync(Guid playerId, CancellationToken ct = default);
}
