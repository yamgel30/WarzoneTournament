using WarzoneTournament.Domain.Common;
using WarzoneTournament.Domain.Enums;

namespace WarzoneTournament.Domain.Entities;

public class EvidenceReview : BaseEntity
{
    public Guid EvidenceId { get; set; }
    public string ReviewedBy { get; set; } = string.Empty;
    public EvidenceStatus Decision { get; set; }
    public string? Notes { get; set; }
    public DateTime ReviewedAt { get; set; } = DateTime.UtcNow;

    public MatchEvidence Evidence { get; set; } = null!;
}
