using WarzoneTournament.Domain.Enums;

namespace WarzoneTournament.Application.DTOs.Match;

public class MatchDto
{
    public Guid Id { get; set; }
    public Guid RoundId { get; set; }
    public string? RoundName { get; set; }
    public Guid TournamentId { get; set; }
    public string? TournamentName { get; set; }
    public int MatchNumber { get; set; }
    public MatchStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public DateTime? ScheduledTime { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? LobbyCode { get; set; }
    public string? LobbyPassword { get; set; }
    public string? MapName { get; set; }
    public string? Notes { get; set; }
    public bool ResultsConfirmed { get; set; }
    public List<MatchTeamResultDto> TeamResults { get; set; } = new();
    public int EvidenceCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MatchTeamResultDto
{
    public Guid Id { get; set; }
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string? TeamTag { get; set; }
    public string? TeamLogoUrl { get; set; }
    public int Placement { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public double PlacementMultiplier { get; set; }
    public double KillPoints { get; set; }
    public int BonusPoints { get; set; }
    public double TotalPoints { get; set; }
    public bool IsVerified { get; set; }
}
