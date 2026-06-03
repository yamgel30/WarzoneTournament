using WarzoneTournament.Application.Common.Interfaces;
using WarzoneTournament.Application.Common.Models;
using WarzoneTournament.Application.DTOs.Team;
using WarzoneTournament.Domain.Entities;
using WarzoneTournament.Domain.Enums;
using WarzoneTournament.Domain.Interfaces;

namespace WarzoneTournament.Application.Services;

public class TeamInvitationService : ITeamInvitationService
{
    private readonly IUnitOfWork _uow;
    private readonly ITeamService _teamService;
    private readonly INotificationService _notifications;
    private readonly IDiscordNotificationService _discord;

    public TeamInvitationService(IUnitOfWork uow, ITeamService teamService,
        INotificationService notifications, IDiscordNotificationService discord)
    {
        _uow = uow;
        _teamService = teamService;
        _notifications = notifications;
        _discord = discord;
    }

    public async Task<Result> SendAsync(Guid teamId, Guid captainPlayerId, Guid invitedPlayerId, string? message, CancellationToken ct = default)
    {
        var team = await _uow.Teams.GetByIdAsync(teamId, ct);
        if (team is null) return Result.Failure("Equipo no encontrado.");
        if (team.CaptainId != captainPlayerId) return Result.Failure("Solo el capitán puede enviar invitaciones.");

        var alreadyMember = await _uow.TeamPlayers.ExistsAsync(tp => tp.TeamId == teamId && tp.PlayerId == invitedPlayerId && tp.IsActive, ct);
        if (alreadyMember) return Result.Failure("El jugador ya es miembro de este equipo.");

        var pendingExists = await _uow.TeamInvitations.ExistsAsync(i =>
            i.TeamId == teamId && i.InvitedPlayerId == invitedPlayerId && i.Status == InvitationStatus.Pending, ct);
        if (pendingExists) return Result.Failure("Ya existe una invitación pendiente para este jugador.");

        var invitation = new TeamInvitation
        {
            TeamId = teamId,
            InvitedPlayerId = invitedPlayerId,
            InvitedByPlayerId = captainPlayerId,
            Message = message,
            Status = InvitationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        await _uow.TeamInvitations.AddAsync(invitation, ct);
        await _uow.SaveChangesAsync(ct);

        // In-app notification
        var appUser = await _uow.AppUsers.FirstOrDefaultAsync(u => u.PlayerId == invitedPlayerId, ct);
        if (appUser is not null)
        {
            var captain = await _uow.Players.GetByIdAsync(captainPlayerId, ct);
            await _notifications.CreateAsync(
                appUser.Id,
                $"Invitación de {team.Name}",
                $"{captain?.Username ?? "Alguien"} te invitó a unirte al equipo {team.Name}.",
                "Invitation",
                "/invitations",
                ct);
        }

        // Discord DM
        var invitedPlayer = await _uow.Players.GetByIdAsync(invitedPlayerId, ct);
        if (!string.IsNullOrEmpty(invitedPlayer?.DiscordId))
        {
            var captain = await _uow.Players.GetByIdAsync(captainPlayerId, ct);
            await _discord.SendDirectMessageAsync(
                invitedPlayer.DiscordId,
                $"🎮 **{team.Name}** te invitó a unirse al equipo!\n" +
                $"Capitán: **{captain?.Username ?? "—"}**\n" +
                $"Entra a la plataforma para aceptar o rechazar la invitación.",
                ct);
        }

        return Result.Success();
    }

    public async Task<Result> SendDiscordInviteAsync(Guid teamId, Guid captainPlayerId, string discordId, string? message, CancellationToken ct = default)
    {
        var team = await _uow.Teams.GetByIdAsync(teamId, ct);
        if (team is null) return Result.Failure("Equipo no encontrado.");
        if (team.CaptainId != captainPlayerId) return Result.Failure("Solo el capitán puede enviar invitaciones.");

        var alreadyPending = await _uow.PendingDiscordInvites.ExistsAsync(
            p => p.TeamId == teamId && p.InvitedDiscordId == discordId && !p.IsConsumed && p.ExpiresAt > DateTime.UtcNow, ct);
        if (alreadyPending) return Result.Failure("Ya existe una invitación pendiente para ese usuario de Discord.");

        var pending = new PendingDiscordInvite
        {
            TeamId = teamId,
            InvitedByPlayerId = captainPlayerId,
            InvitedDiscordId = discordId,
            Message = message,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        await _uow.PendingDiscordInvites.AddAsync(pending, ct);
        await _uow.SaveChangesAsync(ct);

        var captain = await _uow.Players.GetByIdAsync(captainPlayerId, ct);
        await _discord.SendDirectMessageAsync(discordId,
            $"🎮 **{team.Name}** te invitó a unirte a su equipo en Warzone Tournament!\n" +
            $"Capitán: **{captain?.Username ?? "—"}**\n" +
            $"Regístrate en la plataforma con tu cuenta de Discord para aceptar la invitación.\n" +
            (string.IsNullOrEmpty(message) ? "" : $"\nMensaje: _{message}_"),
            ct);

        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<TeamInvitationDto>>> GetPendingForPlayerAsync(Guid playerId, CancellationToken ct = default)
    {
        var invitations = await _uow.TeamInvitations.FindAsync(
            i => i.InvitedPlayerId == playerId && i.Status == InvitationStatus.Pending && i.ExpiresAt > DateTime.UtcNow, ct);
        var dtos = new List<TeamInvitationDto>();
        foreach (var inv in invitations)
        {
            var team = await _uow.Teams.GetByIdAsync(inv.TeamId, ct);
            var invitedBy = await _uow.Players.GetByIdAsync(inv.InvitedByPlayerId, ct);
            dtos.Add(new TeamInvitationDto
            {
                Id = inv.Id,
                TeamId = inv.TeamId,
                TeamName = team?.Name ?? "—",
                TeamLogoUrl = team?.LogoUrl,
                InvitedPlayerId = inv.InvitedPlayerId,
                InvitedPlayerUsername = "—",
                InvitedByPlayerId = inv.InvitedByPlayerId,
                InvitedByUsername = invitedBy?.Username ?? "—",
                Status = inv.Status,
                Message = inv.Message,
                ExpiresAt = inv.ExpiresAt,
                CreatedAt = inv.CreatedAt
            });
        }
        return Result.Success<IReadOnlyList<TeamInvitationDto>>(dtos);
    }

    public async Task<Result<IReadOnlyList<TeamInvitationDto>>> GetSentByTeamAsync(Guid teamId, CancellationToken ct = default)
    {
        var invitations = await _uow.TeamInvitations.FindAsync(i => i.TeamId == teamId, ct);
        var team = await _uow.Teams.GetByIdAsync(teamId, ct);
        var dtos = new List<TeamInvitationDto>();
        foreach (var inv in invitations.OrderByDescending(i => i.CreatedAt))
        {
            var invitedPlayer = await _uow.Players.GetByIdAsync(inv.InvitedPlayerId, ct);
            dtos.Add(new TeamInvitationDto
            {
                Id = inv.Id,
                TeamId = inv.TeamId,
                TeamName = team?.Name ?? "—",
                TeamLogoUrl = team?.LogoUrl,
                InvitedPlayerId = inv.InvitedPlayerId,
                InvitedPlayerUsername = invitedPlayer?.Username ?? "—",
                InvitedByPlayerId = inv.InvitedByPlayerId,
                InvitedByUsername = "—",
                Status = inv.Status,
                Message = inv.Message,
                ExpiresAt = inv.ExpiresAt,
                CreatedAt = inv.CreatedAt
            });
        }
        return Result.Success<IReadOnlyList<TeamInvitationDto>>(dtos);
    }

    public async Task<Result> AcceptAsync(Guid invitationId, Guid playerId, CancellationToken ct = default)
    {
        var inv = await _uow.TeamInvitations.GetByIdAsync(invitationId, ct);
        if (inv is null) return Result.Failure("Invitación no encontrada.");
        if (inv.InvitedPlayerId != playerId) return Result.Failure("No autorizado.");
        if (inv.Status != InvitationStatus.Pending) return Result.Failure("Esta invitación ya fue procesada.");
        if (inv.ExpiresAt < DateTime.UtcNow)
        {
            inv.Status = InvitationStatus.Expired;
            _uow.TeamInvitations.Update(inv);
            await _uow.SaveChangesAsync(ct);
            return Result.Failure("La invitación expiró.");
        }

        inv.Status = InvitationStatus.Accepted;
        _uow.TeamInvitations.Update(inv);
        await _uow.SaveChangesAsync(ct);

        return await _teamService.AddPlayerToTeamAsync(inv.TeamId, playerId, ct);
    }

    public async Task<Result> DeclineAsync(Guid invitationId, Guid playerId, CancellationToken ct = default)
    {
        var inv = await _uow.TeamInvitations.GetByIdAsync(invitationId, ct);
        if (inv is null) return Result.Failure("Invitación no encontrada.");
        if (inv.InvitedPlayerId != playerId) return Result.Failure("No autorizado.");
        if (inv.Status != InvitationStatus.Pending) return Result.Failure("Esta invitación ya fue procesada.");

        inv.Status = InvitationStatus.Declined;
        _uow.TeamInvitations.Update(inv);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<int> CountPendingForPlayerAsync(Guid playerId, CancellationToken ct = default)
    {
        return await _uow.TeamInvitations.CountAsync(
            i => i.InvitedPlayerId == playerId && i.Status == InvitationStatus.Pending && i.ExpiresAt > DateTime.UtcNow, ct);
    }
}
