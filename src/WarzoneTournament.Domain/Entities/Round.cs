using WarzoneTournament.Domain.Common;

namespace WarzoneTournament.Domain.Entities;

public class Round : BaseEntity
{
    public Guid TournamentId { get; set; }
    public int RoundNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool IsCompleted { get; set; } = false;
    public string? Notes { get; set; }

    public Tournament Tournament { get; set; } = null!;
    public ICollection<Match> Matches { get; set; } = new List<Match>();
}
