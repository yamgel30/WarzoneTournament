using WarzoneTournament.Domain.Common;
using WarzoneTournament.Domain.Enums;

namespace WarzoneTournament.Domain.Entities;

public class Match : BaseEntity
{
    public Guid RoundId { get; set; }
    public Guid TournamentId { get; set; }
    public int MatchNumber { get; set; }
    public MatchStatus Status { get; set; } = MatchStatus.Pending;
    public DateTime? ScheduledTime { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? LobbyCode { get; set; }
    public string? LobbyPassword { get; set; }
    public string? MapName { get; set; }
    public string? Notes { get; set; }
    public bool ResultsConfirmed { get; set; } = false;

    public Round Round { get; set; } = null!;
    public Tournament Tournament { get; set; } = null!;
    public ICollection<MatchTeamResult> TeamResults { get; set; } = new List<MatchTeamResult>();
    public ICollection<PlayerMatchStats> PlayerStats { get; set; } = new List<PlayerMatchStats>();
    public ICollection<MatchEvidence> Evidences { get; set; } = new List<MatchEvidence>();
}
