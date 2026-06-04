namespace WarzoneTournament.Application.DTOs.Settings;

public class SiteSettingsDto
{
    public string? AppName { get; set; }
    public string? SupportEmail { get; set; }
    public string? DefaultLogoUrl { get; set; }
    public string? DefaultBannerUrl { get; set; }
    public string? DefaultPlacementPointsJson { get; set; }
    public int? DefaultMatchPointThreshold { get; set; }
    public string? DiscordBotToken { get; set; }
    public string? DefaultDiscordGuildId { get; set; }
    public string? DefaultDiscordAnnouncementChannelId { get; set; }
    public string? DefaultDiscordEvidenceChannelId { get; set; }
    public Guid? FeaturedTournamentId { get; set; }

    // Discord notification toggles — DMs
    public bool DiscordDmEvidenceApproved { get; set; } = true;
    public bool DiscordDmEvidenceRejected { get; set; } = true;
    public bool DiscordDmTeamInvitation { get; set; } = true;
    public bool DiscordDmPendingInvite { get; set; } = true;

    // Discord notification toggles — channel announcements
    public bool DiscordAnnounceTournamentStart { get; set; } = true;
    public bool DiscordAnnounceMatchResult { get; set; } = true;
    public bool DiscordAnnounceNewRegistration { get; set; } = false;
}
