using WarzoneTournament.Domain.Enums;

namespace WarzoneTournament.Application.DTOs.Auth;

public class AppUserDto
{
    public Guid Id { get; set; }
    public string DiscordId { get; set; } = string.Empty;
    public string DiscordUsername { get; set; } = string.Empty;
    public string? AvatarHash { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public UserRole Role { get; set; }
    public Guid? PlayerId { get; set; }
    public bool ProfileComplete => PlayerId.HasValue;
    public string AvatarUrl => string.IsNullOrEmpty(AvatarHash)
        ? "https://cdn.discordapp.com/embed/avatars/0.png"
        : $"https://cdn.discordapp.com/avatars/{DiscordId}/{AvatarHash}.png";
}
