using WarzoneTournament.Application.Common.Models;
using WarzoneTournament.Application.DTOs.Team;

namespace WarzoneTournament.Application.Common.Interfaces;

public interface ITeamInvitationService
{
    Task<Result> SendAsync(Guid teamId, Guid captainPlayerId, Guid invitedPlayerId, string? message, CancellationToken ct = default);
    Task<Result<IReadOnlyList<TeamInvitationDto>>> GetPendingForPlayerAsync(Guid playerId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<TeamInvitationDto>>> GetSentByTeamAsync(Guid teamId, CancellationToken ct = default);
    Task<Result> AcceptAsync(Guid invitationId, Guid playerId, CancellationToken ct = default);
    Task<Result> DeclineAsync(Guid invitationId, Guid playerId, CancellationToken ct = default);
    Task<int> CountPendingForPlayerAsync(Guid playerId, CancellationToken ct = default);
}
