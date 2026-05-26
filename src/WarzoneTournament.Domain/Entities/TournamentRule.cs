using WarzoneTournament.Domain.Common;

namespace WarzoneTournament.Domain.Entities;

public class TournamentRule : BaseEntity
{
    public Guid TournamentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Order { get; set; } = 0;
    public bool IsRequired { get; set; } = false;

    public Tournament Tournament { get; set; } = null!;
}
