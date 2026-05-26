namespace WarzoneTournament.Application.DTOs.Player;

public class PlayerStatsDto
{
    public Guid PlayerId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int TotalKills { get; set; }
    public int TotalDeaths { get; set; }
    public int TotalAssists { get; set; }
    public int TotalDamage { get; set; }
    public int TotalMatches { get; set; }
    public int TotalWins { get; set; }
    public int TotalRevives { get; set; }
    public double KdRatio => TotalDeaths > 0 ? Math.Round((double)TotalKills / TotalDeaths, 2) : TotalKills;
    public double WinRate => TotalMatches > 0 ? Math.Round((double)TotalWins / TotalMatches * 100, 1) : 0;
    public double AverageKillsPerMatch => TotalMatches > 0 ? Math.Round((double)TotalKills / TotalMatches, 1) : 0;
    public double AverageDamagePerMatch => TotalMatches > 0 ? Math.Round((double)TotalDamage / TotalMatches, 0) : 0;
    public int BestKillGame { get; set; }
    public List<RecentMatchStatsDto> RecentMatches { get; set; } = new();
}

public class RecentMatchStatsDto
{
    public Guid MatchId { get; set; }
    public string? TournamentName { get; set; }
    public DateTime? MatchDate { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Damage { get; set; }
    public int TeamPlacement { get; set; }
}
