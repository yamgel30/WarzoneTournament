using WarzoneTournament.Domain.Common;
using WarzoneTournament.Domain.Enums;

namespace WarzoneTournament.Domain.Entities;

public class Player : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string? ActivisionId { get; set; }
    public string? DiscordId { get; set; }
    public string? DiscordUsername { get; set; }
    public string? Email { get; set; }
    public Platform Platform { get; set; }
    public string? Country { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsVerified { get; set; } = false;
    public bool IsBanned { get; set; } = false;
    public string? BanReason { get; set; }
    public int TotalKills { get; set; } = 0;
    public int TotalDeaths { get; set; } = 0;
    public int TotalMatches { get; set; } = 0;
    public int TotalWins { get; set; } = 0;

    public ICollection<TeamPlayer> TeamPlayers { get; set; } = new List<TeamPlayer>();
    public ICollection<PlayerMatchStats> MatchStats { get; set; } = new List<PlayerMatchStats>();
}
