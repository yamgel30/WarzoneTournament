using WarzoneTournament.Domain.Enums;

namespace WarzoneTournament.Application.DTOs.Team;

public class CreateTeamDto
{
    public string Name { get; set; } = string.Empty;
    public string? Tag { get; set; }
    public string? LogoUrl { get; set; }
    public string? Country { get; set; }
    public string? ContactEmail { get; set; }
    public Platform PreferredPlatform { get; set; }
    public Guid CaptainId { get; set; }
    public List<Guid> PlayerIds { get; set; } = new();
}
