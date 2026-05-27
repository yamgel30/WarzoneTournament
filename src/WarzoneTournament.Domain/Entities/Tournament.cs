using WarzoneTournament.Domain.Common;
using WarzoneTournament.Domain.Enums;

namespace WarzoneTournament.Domain.Entities;

public class Tournament : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TournamentType Type { get; set; }
    public TournamentStatus Status { get; set; } = TournamentStatus.Draft;
    public Platform Platform { get; set; }
    public int MaxTeams { get; set; }
    public int PlayersPerTeam { get; set; }
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
    public int? MatchPointThreshold { get; set; }
    public Guid? WinnerTeamId { get; set; }
    public bool IsPrivate { get; set; } = false;
    public string? LobbyCode { get; set; }
    public string? LobbyPassword { get; set; }
    public string? BannerImageUrl { get; set; }
    public string? OrganizerName { get; set; }

    public ICollection<TournamentTeam> TournamentTeams { get; set; } = new List<TournamentTeam>();
    public ICollection<Round> Rounds { get; set; } = new List<Round>();
    public ICollection<TournamentRule> TournamentRules { get; set; } = new List<TournamentRule>();
    public ICollection<PrizeDistribution> PrizeDistributions { get; set; } = new List<PrizeDistribution>();
    public ICollection<Match> Matches { get; set; } = new List<Match>();
}
