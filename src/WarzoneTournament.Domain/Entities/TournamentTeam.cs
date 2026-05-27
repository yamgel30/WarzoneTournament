using WarzoneTournament.Domain.Common;

namespace WarzoneTournament.Domain.Entities;

public class TournamentTeam : BaseEntity
{
    public Guid TournamentId { get; set; }
    public Guid TeamId { get; set; }
    public int Seed { get; set; } = 0;
    public bool CheckedIn { get; set; } = false;
    public DateTime? CheckInTime { get; set; }
    public bool IsEliminated { get; set; } = false;
    public bool IsMatchPoint { get; set; } = false;
    public double TotalPoints { get; set; } = 0;
    public int TotalKills { get; set; } = 0;
    public int CurrentRank { get; set; } = 0;

    public Tournament Tournament { get; set; } = null!;
    public Team Team { get; set; } = null!;
}
