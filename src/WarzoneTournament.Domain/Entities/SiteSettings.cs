using WarzoneTournament.Domain.Common;

namespace WarzoneTournament.Domain.Entities;

public class SiteSettings : BaseEntity
{
    // Branding
    public string? AppName { get; set; }
    public string? SupportEmail { get; set; }

    // Tournament defaults applied when creating a new tournament
    public string? DefaultLogoUrl { get; set; }
    public string? DefaultBannerUrl { get; set; }
    public string? DefaultPlacementPointsJson { get; set; }
    public int? DefaultMatchPointThreshold { get; set; }

    // Discord bot credentials
    public string? DiscordBotToken { get; set; }

    // Discord global defaults
    public string? DefaultDiscordGuildId { get; set; }
    public string? DefaultDiscordAnnouncementChannelId { get; set; }
    public string? DefaultDiscordEvidenceChannelId { get; set; }

    // Public leaderboard
    public Guid? FeaturedTournamentId { get; set; }

    // Discord notification toggles — DMs to players
    public bool DiscordDmEvidenceApproved { get; set; } = true;
    public bool DiscordDmEvidenceRejected { get; set; } = true;
    public bool DiscordDmTeamInvitation { get; set; } = true;
    public bool DiscordDmPendingInvite { get; set; } = true;

    // Discord notification toggles — channel announcements
    public bool DiscordAnnounceTournamentStart { get; set; } = true;
    public bool DiscordAnnounceMatchResult { get; set; } = true;
    public bool DiscordAnnounceNewRegistration { get; set; } = false;
}
