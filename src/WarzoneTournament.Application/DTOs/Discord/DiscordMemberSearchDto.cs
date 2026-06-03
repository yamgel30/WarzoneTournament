namespace WarzoneTournament.Application.DTOs.Discord;

public class DiscordMemberSearchDto
{
    public string DiscordId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? GlobalName { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsRegistered { get; set; }
    public Guid? PlayerId { get; set; }
    public string DisplayName => !string.IsNullOrEmpty(GlobalName) ? GlobalName : Username;
}
