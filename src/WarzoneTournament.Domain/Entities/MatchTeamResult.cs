using WarzoneTournament.Domain.Common;

namespace WarzoneTournament.Domain.Entities;

public class MatchTeamResult : BaseEntity
{
    public Guid MatchId { get; set; }
    public Guid TeamId { get; set; }
    public int Placement { get; set; } = 0;
    public int Kills { get; set; } = 0;
    public int Deaths { get; set; } = 0;
    public double PlacementMultiplier { get; set; } = 1.0;
    public double KillPoints { get; set; } = 0;
    public int BonusPoints { get; set; } = 0;
    public double TotalPoints { get; set; } = 0;
    public bool IsVerified { get; set; } = false;

    public Match Match { get; set; } = null!;
    public Team Team { get; set; } = null!;
}
