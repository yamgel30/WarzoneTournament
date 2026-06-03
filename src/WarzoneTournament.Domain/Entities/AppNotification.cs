using WarzoneTournament.Domain.Common;

namespace WarzoneTournament.Domain.Entities;

public class AppNotification : BaseEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Link { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;

    public AppUser? User { get; set; }
}
