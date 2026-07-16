using AutoMapper;
using WarzoneTournament.Application.Common.Interfaces;
using WarzoneTournament.Application.Common.Models;
using WarzoneTournament.Application.DTOs.Auth;
using WarzoneTournament.Domain.Entities;
using WarzoneTournament.Domain.Enums;
using WarzoneTournament.Domain.Interfaces;

namespace WarzoneTournament.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UserService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<AppUserDto> FindOrCreateByDiscordAsync(
        string discordId, string discordUsername, string? email, string? avatarHash,
        CancellationToken ct = default)
    {
        var existing = (await _uow.AppUsers.FindAsync(u => u.DiscordId == discordId, ct)).FirstOrDefault();
        if (existing is not null)
        {
            existing.DiscordUsername = discordUsername;
            existing.AvatarHash = avatarHash;
            if (email is not null) existing.Email = email;

            if (existing.PlayerId is null)
            {
                var linkedPlayer = (await _uow.Players.FindAsync(p => p.DiscordId == discordId, ct)).FirstOrDefault();
                if (linkedPlayer is not null)
                {
                    existing.PlayerId = linkedPlayer.Id;
                    if (string.IsNullOrEmpty(existing.DisplayName) || existing.DisplayName == existing.DiscordUsername)
                        existing.DisplayName = linkedPlayer.Username;
                }
            }

            _uow.AppUsers.Update(existing);
            await _uow.SaveChangesAsync(ct);

            if (existing.PlayerId.HasValue)
                await ConsumePendingDiscordInvitesAsync(discordId, existing.PlayerId.Value, ct);

            return _mapper.Map<AppUserDto>(existing);
        }

        var existingPlayer = (await _uow.Players.FindAsync(p => p.DiscordId == discordId, ct)).FirstOrDefault();

        var user = new AppUser
        {
            DiscordId       = discordId,
            DiscordUsername = discordUsername,
            AvatarHash      = avatarHash,
            DisplayName     = existingPlayer?.Username ?? discordUsername,
            Email           = email ?? existingPlayer?.Email,
            Role            = UserRole.Player,
            PlayerId        = existingPlayer?.Id
        };
        await _uow.AppUsers.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        if (user.PlayerId.HasValue)
            await ConsumePendingDiscordInvitesAsync(discordId, user.PlayerId.Value, ct);

        return _mapper.Map<AppUserDto>(user);
    }

    public async Task<AppUserDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _uow.AppUsers.GetByIdAsync(id, ct);
        return user is null ? null : _mapper.Map<AppUserDto>(user);
    }

    public async Task<AppUserDto?> GetByDiscordIdAsync(string discordId, CancellationToken ct = default)
    {
        var user = (await _uow.AppUsers.FindAsync(u => u.DiscordId == discordId, ct)).FirstOrDefault();
        return user is null ? null : _mapper.Map<AppUserDto>(user);
    }

    public async Task<Result<AppUserDto>> CompleteProfileAsync(
        Guid userId, string displayName, string gamerTag, Platform platform,
        CancellationToken ct = default)
    {
        var user = await _uow.AppUsers.GetByIdAsync(userId, ct);
        if (user is null) return Result.Failure<AppUserDto>("User not found.");

        // Create the linked Player if doesn't exist yet
        if (user.PlayerId is null)
        {
            var player = new Player
            {
                Username        = gamerTag,
                DiscordId       = user.DiscordId,
                DiscordUsername = user.DiscordUsername,
                Email           = user.Email,
                Platform        = platform,
                AvatarUrl       = string.IsNullOrEmpty(user.AvatarHash)
                    ? null
                    : $"https://cdn.discordapp.com/avatars/{user.DiscordId}/{user.AvatarHash}.png"
            };
            await _uow.Players.AddAsync(player, ct);
            user.PlayerId = player.Id;
        }

        user.DisplayName = displayName;
        _uow.AppUsers.Update(user);
        await _uow.SaveChangesAsync(ct);

        if (user.PlayerId.HasValue)
            await ConsumePendingDiscordInvitesAsync(user.DiscordId, user.PlayerId.Value, ct);

        return Result.Success(_mapper.Map<AppUserDto>(user));
    }

    private async Task ConsumePendingDiscordInvitesAsync(string discordId, Guid playerId, CancellationToken ct)
    {
        var pending = await _uow.PendingDiscordInvites.FindAsync(
            p => p.InvitedDiscordId == discordId && !p.IsConsumed && p.ExpiresAt > DateTime.UtcNow, ct);
        foreach (var invite in pending)
        {
            var alreadyExists = await _uow.TeamInvitations.ExistsAsync(i =>
                i.TeamId == invite.TeamId && i.InvitedPlayerId == playerId && i.Status == InvitationStatus.Pending, ct);
            if (!alreadyExists)
            {
                await _uow.TeamInvitations.AddAsync(new TeamInvitation
                {
                    TeamId = invite.TeamId,
                    InvitedPlayerId = playerId,
                    InvitedByPlayerId = invite.InvitedByPlayerId,
                    Message = invite.Message,
                    Status = InvitationStatus.Pending,
                    ExpiresAt = invite.ExpiresAt
                }, ct);
            }
            invite.IsConsumed = true;
            _uow.PendingDiscordInvites.Update(invite);
        }
        if (pending.Any()) await _uow.SaveChangesAsync(ct);
    }
}
