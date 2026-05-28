using WarzoneTournament.Domain.Enums;

namespace WarzoneTournament.Application.DTOs.Tournament;

public class UpdateTournamentDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public Platform? Platform { get; set; }
    public int? MaxTeams { get; set; }
    public int? PlayersPerTeam { get; set; }
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
    public int? KillPoints { get; set; }
    public string? PlacementPointsJson { get; set; }
    public bool? IsPrivate { get; set; }
    public string? LobbyCode { get; set; }
    public string? LobbyPassword { get; set; }
    public string? BannerImageUrl { get; set; }
    public string? LogoUrl { get; set; }
    public string? OrganizerName { get; set; }
    public int? MatchPointThreshold { get; set; }
}
