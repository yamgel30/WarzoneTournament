using WarzoneTournament.Domain.Enums;

namespace WarzoneTournament.Application.DTOs.Player;

public class PlayerDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? ActivisionId { get; set; }
    public string? DiscordId { get; set; }
    public string? DiscordUsername { get; set; }
    public string? Email { get; set; }
    public Platform Platform { get; set; }
    public string PlatformName => Platform.ToString();
    public string? Country { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsVerified { get; set; }
    public bool IsBanned { get; set; }
    public string? BanReason { get; set; }
    public int TotalKills { get; set; }
    public int TotalDeaths { get; set; }
    public int TotalMatches { get; set; }
    public int TotalWins { get; set; }
    public double KdRatio => TotalDeaths > 0 ? Math.Round((double)TotalKills / TotalDeaths, 2) : TotalKills;
    public double WinRate => TotalMatches > 0 ? Math.Round((double)TotalWins / TotalMatches * 100, 1) : 0;
    public DateTime CreatedAt { get; set; }
}
