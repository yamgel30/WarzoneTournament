namespace WarzoneTournament.Application.DTOs.Evidence;

public class DiscordEvidenceDto
{
    public Guid MatchId { get; set; }
    public Guid SubmittedByTeamId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string DiscordMessageId { get; set; } = string.Empty;
    public string DiscordChannelId { get; set; } = string.Empty;
    public string? DiscordUsername { get; set; }
    public string? OriginalFileName { get; set; }
    public long? FileSizeBytes { get; set; }
}
