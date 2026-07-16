using WarzoneTournament.Domain.Common;

namespace WarzoneTournament.Domain.Entities;

public class PendingDiscordInvite : BaseEntity
{
    public Guid TeamId { get; set; }
    public Guid InvitedByPlayerId { get; set; }
    public string InvitedDiscordId { get; set; } = string.Empty;
    public string? Message { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsConsumed { get; set; } = false;

    public Team? Team { get; set; }
    public Player? InvitedByPlayer { get; set; }
}
