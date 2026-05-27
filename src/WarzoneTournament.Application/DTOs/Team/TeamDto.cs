using WarzoneTournament.Domain.Enums;

namespace WarzoneTournament.Application.DTOs.Team;

public class TeamDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Tag { get; set; }
    public string? LogoUrl { get; set; }
    public string? Country { get; set; }
    public string? DiscordRoleId { get; set; }
    public string? ContactEmail { get; set; }
    public Platform PreferredPlatform { get; set; }
    public string PlatformName => PreferredPlatform.ToString();
    public Guid CaptainId { get; set; }
    public string? CaptainUsername { get; set; }
    public int PlayerCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<TeamPlayerDto> Players { get; set; } = new();
}

public class TeamPlayerDto
{
    public Guid PlayerId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? ActivisionId { get; set; }
    public string? DiscordUsername { get; set; }
    public Platform Platform { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Role { get; set; }
    public DateTime JoinedAt { get; set; }
}

public class TournamentTeamStatusDto
{
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string? TeamTag { get; set; }
    public string? LogoUrl { get; set; }
    public int PlayerCount { get; set; }
    public bool IsRegistered { get; set; }
    public bool CheckedIn { get; set; }
    public DateTime? CheckInTime { get; set; }
    public List<TeamPlayerSimpleDto> Players { get; set; } = new();
}

public class TeamPlayerSimpleDto
{
    public Guid PlayerId { get; set; }
    public string Username { get; set; } = string.Empty;
}
