using WarzoneTournament.Domain.Enums;

namespace WarzoneTournament.Application.DTOs.Player;

public class CreatePlayerDto
{
    public string Username { get; set; } = string.Empty;
    public string? ActivisionId { get; set; }
    public string? DiscordId { get; set; }
    public string? DiscordUsername { get; set; }
    public string? Email { get; set; }
    public Platform Platform { get; set; }
    public string? Country { get; set; }
    public string? AvatarUrl { get; set; }
}
