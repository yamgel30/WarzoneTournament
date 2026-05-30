namespace WarzoneTournament.Application.DTOs.Discord;

public class DiscordUserDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? GlobalName { get; set; }
    public string? AvatarUrl { get; set; }
    public string DisplayName => !string.IsNullOrEmpty(GlobalName) ? GlobalName : Username;
}
