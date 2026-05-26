using WarzoneTournament.Application.DTOs.Match;

namespace WarzoneTournament.Application.DTOs.Round;

public class RoundDto
{
    public Guid Id { get; set; }
    public Guid TournamentId { get; set; }
    public int RoundNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool IsCompleted { get; set; }
    public string? Notes { get; set; }
    public List<MatchDto> Matches { get; set; } = new();
}

public class CreateRoundDto
{
    public Guid TournamentId { get; set; }
    public int RoundNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? StartTime { get; set; }
    public string? Notes { get; set; }
}
