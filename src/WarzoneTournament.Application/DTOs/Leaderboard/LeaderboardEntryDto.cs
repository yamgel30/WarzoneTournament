namespace WarzoneTournament.Application.DTOs.Leaderboard;

public class LeaderboardEntryDto
{
    public int Rank { get; set; }
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string? TeamTag { get; set; }
    public string? TeamLogoUrl { get; set; }
    public int TotalPoints { get; set; }
    public int TotalKills { get; set; }
    public int TotalPlacementPoints { get; set; }
    public int TotalKillPoints { get; set; }
    public int TotalBonusPoints { get; set; }
    public int MatchesPlayed { get; set; }
    public int BestPlacement { get; set; }
    public int AverageKillsPerMatch => MatchesPlayed > 0 ? TotalKills / MatchesPlayed : 0;
    public bool CheckedIn { get; set; }
    public bool IsEliminated { get; set; }
    public List<MatchScoreDto> MatchScores { get; set; } = new();
}

public class MatchScoreDto
{
    public Guid MatchId { get; set; }
    public int MatchNumber { get; set; }
    public int Placement { get; set; }
    public int Kills { get; set; }
    public int Points { get; set; }
}
