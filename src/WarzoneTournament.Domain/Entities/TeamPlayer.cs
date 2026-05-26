using WarzoneTournament.Domain.Common;

namespace WarzoneTournament.Domain.Entities;

public class TeamPlayer : BaseEntity
{
    public Guid TeamId { get; set; }
    public Guid PlayerId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }
    public string? Role { get; set; }

    public Team Team { get; set; } = null!;
    public Player Player { get; set; } = null!;
}
