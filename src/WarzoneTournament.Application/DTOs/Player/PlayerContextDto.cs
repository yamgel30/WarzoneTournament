using WarzoneTournament.Application.DTOs.Team;

namespace WarzoneTournament.Application.DTOs.Player;

public class PlayerContextDto
{
    public Guid PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public Guid TournamentId { get; set; }
    public string TournamentName { get; set; } = string.Empty;
    public Guid? ActiveMatchId { get; set; }
    public int ActiveMatchNumber { get; set; }
    public List<TeamPlayerSimpleDto> Players { get; set; } = new();
}
