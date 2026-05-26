using WarzoneTournament.Domain.Common;

namespace WarzoneTournament.Domain.Entities;

public class PlayerMatchStats : BaseEntity
{
    public Guid MatchId { get; set; }
    public Guid PlayerId { get; set; }
    public Guid TeamId { get; set; }
    public int Kills { get; set; } = 0;
    public int Deaths { get; set; } = 0;
    public int Assists { get; set; } = 0;
    public int Damage { get; set; } = 0;
    public decimal? HeadshotPercentage { get; set; }
    public int? GulagWins { get; set; }
    public int? GulagAttempts { get; set; }
    public int Revives { get; set; } = 0;
    public int DistanceTraveled { get; set; } = 0;
    public bool IsVerified { get; set; } = false;

    public Match Match { get; set; } = null!;
    public Player Player { get; set; } = null!;
    public Team Team { get; set; } = null!;
}
