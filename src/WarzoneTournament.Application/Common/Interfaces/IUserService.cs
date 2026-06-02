using WarzoneTournament.Application.Common.Models;
using WarzoneTournament.Application.DTOs.Auth;
using WarzoneTournament.Domain.Enums;

namespace WarzoneTournament.Application.Common.Interfaces;

public interface IUserService
{
    Task<AppUserDto> FindOrCreateByDiscordAsync(string discordId, string discordUsername, string? email, string? avatarHash, CancellationToken ct = default);
    Task<AppUserDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AppUserDto?> GetByDiscordIdAsync(string discordId, CancellationToken ct = default);
    Task<Result<AppUserDto>> CompleteProfileAsync(Guid userId, string displayName, string gamerTag, Platform platform, CancellationToken ct = default);
}
