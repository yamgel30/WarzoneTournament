using WarzoneTournament.Domain.Enums;

namespace WarzoneTournament.Application.DTOs.Player;

public class PlayerListDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? DiscordUsername { get; set; }
    public Platform Platform { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Country { get; set; }
    public bool IsVerified { get; set; }
}
