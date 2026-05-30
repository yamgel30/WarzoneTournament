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
}
