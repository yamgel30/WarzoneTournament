using WarzoneTournament.Domain.Common;
using WarzoneTournament.Domain.Enums;

namespace WarzoneTournament.Domain.Entities;

public class AppUser : BaseEntity
{
    public string DiscordId { get; set; } = string.Empty;
    public string DiscordUsername { get; set; } = string.Empty;
    public string? AvatarHash { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public UserRole Role { get; set; } = UserRole.Player;
    public Guid? PlayerId { get; set; }
    public Player? Player { get; set; }
}
