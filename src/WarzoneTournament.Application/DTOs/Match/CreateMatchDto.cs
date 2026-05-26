namespace WarzoneTournament.Application.DTOs.Match;

public class CreateMatchDto
{
    public Guid RoundId { get; set; }
    public Guid TournamentId { get; set; }
    public int MatchNumber { get; set; }
    public DateTime? ScheduledTime { get; set; }
    public string? LobbyCode { get; set; }
    public string? LobbyPassword { get; set; }
    public string? MapName { get; set; }
    public string? Notes { get; set; }
}
