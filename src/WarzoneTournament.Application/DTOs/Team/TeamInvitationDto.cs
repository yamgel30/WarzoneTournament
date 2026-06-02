using WarzoneTournament.Domain.Enums;

namespace WarzoneTournament.Application.DTOs.Team;

public class TeamInvitationDto
{
    public Guid Id { get; set; }
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string? TeamLogoUrl { get; set; }
    public Guid InvitedPlayerId { get; set; }
    public string InvitedPlayerUsername { get; set; } = string.Empty;
    public Guid InvitedByPlayerId { get; set; }
    public string InvitedByUsername { get; set; } = string.Empty;
    public InvitationStatus Status { get; set; }
    public string? Message { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
