namespace WarzoneTournament.Application.DTOs.Settings;

public class SiteSettingsDto
{
    public string? AppName { get; set; }
    public string? SupportEmail { get; set; }
    public string? DefaultLogoUrl { get; set; }
    public string? DefaultBannerUrl { get; set; }
    public string? DefaultPlacementPointsJson { get; set; }
    public int? DefaultMatchPointThreshold { get; set; }
    public string? DefaultDiscordGuildId { get; set; }
    public string? DefaultDiscordAnnouncementChannelId { get; set; }
    public string? DefaultDiscordEvidenceChannelId { get; set; }
}
