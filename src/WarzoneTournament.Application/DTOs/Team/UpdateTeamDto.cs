using WarzoneTournament.Domain.Enums;

namespace WarzoneTournament.Application.DTOs.Team;

public class UpdateTeamDto
{
    public string? Name { get; set; }
    public string? Tag { get; set; }
    public string? LogoUrl { get; set; }
    public string? Country { get; set; }
    public string? ContactEmail { get; set; }
    public Platform? PreferredPlatform { get; set; }
}
