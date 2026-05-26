using WarzoneTournament.Domain.Common;

namespace WarzoneTournament.Domain.Entities;

public class PrizeDistribution : BaseEntity
{
    public Guid TournamentId { get; set; }
    public int Position { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string Currency { get; set; } = "USD";

    public Tournament Tournament { get; set; } = null!;
}
