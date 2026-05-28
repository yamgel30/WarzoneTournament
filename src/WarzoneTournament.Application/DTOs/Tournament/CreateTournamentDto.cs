using WarzoneTournament.Domain.Enums;

namespace WarzoneTournament.Application.DTOs.Tournament;

public class CreateTournamentDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TournamentType Type { get; set; }
    public Platform Platform { get; set; }
    public int MaxTeams { get; set; } = 20;
    public int PlayersPerTeam { get; set; } = 3;
    public DateTime? RegistrationStart { get; set; }
    public DateTime? RegistrationEnd { get; set; }
    public DateTime? CheckInStart { get; set; }
    public DateTime? CheckInEnd { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? PrizePool { get; set; }
    public string? TournamentRulesText { get; set; }
    public string? StreamUrl { get; set; }
    public string? DiscordChannelId { get; set; }
    public string? DiscordGuildId { get; set; }
    public int KillPoints { get; set; } = 1;
    public string PlacementPointsJson { get; set; } =
        "{\"1\":1.6,\"2\":1.4,\"3\":1.4,\"4\":1.4,\"5\":1.4,\"6\":1.2,\"7\":1.2,\"8\":1.2,\"9\":1.2,\"10\":1.2,\"11\":1.0,\"12\":1.0,\"13\":1.0,\"14\":1.0,\"15\":1.0,\"16\":1.0,\"17\":1.0}";
    public bool IsPrivate { get; set; } = false;
    public string? LobbyCode { get; set; }
    public string? LobbyPassword { get; set; }
    public string? BannerImageUrl { get; set; }
    public string? LogoUrl { get; set; }
    public string? OrganizerName { get; set; }
    public int? MatchPointThreshold { get; set; }
}
