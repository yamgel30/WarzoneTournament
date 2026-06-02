using WarzoneTournament.Domain.Common;
using WarzoneTournament.Domain.Enums;

namespace WarzoneTournament.Domain.Entities;

public class TeamInvitation : BaseEntity
{
    public Guid TeamId { get; set; }
    public Guid InvitedPlayerId { get; set; }
    public Guid InvitedByPlayerId { get; set; }
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public string? Message { get; set; }
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);

    public Team Team { get; set; } = null!;
    public Player InvitedPlayer { get; set; } = null!;
    public Player InvitedByPlayer { get; set; } = null!;
}
