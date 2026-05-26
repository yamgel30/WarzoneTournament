using WarzoneTournament.Domain.Common;
using WarzoneTournament.Domain.Enums;

namespace WarzoneTournament.Domain.Entities;

public class Team : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Tag { get; set; }
    public string? LogoUrl { get; set; }
    public string? Country { get; set; }
    public string? DiscordRoleId { get; set; }
    public string? ContactEmail { get; set; }
    public Platform PreferredPlatform { get; set; }
    public Guid CaptainId { get; set; }

    public Player Captain { get; set; } = null!;
    public ICollection<TeamPlayer> TeamPlayers { get; set; } = new List<TeamPlayer>();
    public ICollection<TournamentTeam> TournamentTeams { get; set; } = new List<TournamentTeam>();
    public ICollection<MatchTeamResult> MatchResults { get; set; } = new List<MatchTeamResult>();
}
