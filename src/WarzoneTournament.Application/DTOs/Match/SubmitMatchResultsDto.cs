namespace WarzoneTournament.Application.DTOs.Match;

public class SubmitMatchResultsDto
{
    public List<TeamResultDto> TeamResults { get; set; } = new();
    public List<PlayerStatSubmitDto> PlayerStats { get; set; } = new();
}

public class TeamResultDto
{
    public Guid TeamId { get; set; }
    public int Placement { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int BonusPoints { get; set; } = 0;
}

public class PlayerStatSubmitDto
{
    public Guid PlayerId { get; set; }
    public Guid TeamId { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public int Damage { get; set; }
    public decimal? HeadshotPercentage { get; set; }
    public int? GulagWins { get; set; }
    public int? GulagAttempts { get; set; }
    public int Revives { get; set; }
    public int DistanceTraveled { get; set; }
}
