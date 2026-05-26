using WarzoneTournament.Domain.Enums;

namespace WarzoneTournament.Application.DTOs.Player;

public class UpdatePlayerDto
{
    public string? Username { get; set; }
    public string? ActivisionId { get; set; }
    public string? DiscordId { get; set; }
    public string? DiscordUsername { get; set; }
    public string? Email { get; set; }
    public Platform? Platform { get; set; }
    public string? Country { get; set; }
    public string? AvatarUrl { get; set; }
}
