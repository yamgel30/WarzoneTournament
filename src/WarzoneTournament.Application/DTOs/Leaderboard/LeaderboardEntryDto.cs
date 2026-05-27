namespace WarzoneTournament.Application.DTOs.Leaderboard;

public class LeaderboardEntryDto
{
    public int Rank { get; set; }
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string? TeamTag { get; set; }
    public string? TeamLogoUrl { get; set; }
    public double TotalPoints { get; set; }
    public int TotalKills { get; set; }
    public double TotalKillPoints { get; set; }
    public int TotalBonusPoints { get; set; }
    public int MatchesPlayed { get; set; }
    public int BestPlacement { get; set; }
    public int AverageKillsPerMatch => MatchesPlayed > 0 ? TotalKills / MatchesPlayed : 0;
    public bool CheckedIn { get; set; }
    public bool IsEliminated { get; set; }
    public bool IsMatchPoint { get; set; }
    public List<MatchScoreDto> MatchScores { get; set; } = new();
}

public class MatchScoreDto
{
    public Guid MatchId { get; set; }
    public int MatchNumber { get; set; }
    public int Placement { get; set; }
    public int Kills { get; set; }
    public double Points { get; set; }
}

public class PlayerLeaderboardEntryDto
{
    public int Rank { get; set; }
    public Guid PlayerId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? TeamName { get; set; }
    public string? TeamTag { get; set; }
    public int TotalKills { get; set; }
    public int MatchesPlayed { get; set; }
    public double KillsPerMatch => MatchesPlayed > 0 ? Math.Round((double)TotalKills / MatchesPlayed, 1) : 0;
}
