using WarzoneTournament.Domain.Enums;

namespace WarzoneTournament.Application.DTOs.Tournament;

public class TournamentListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TournamentType Type { get; set; }
    public string TypeName => Type.ToString();
    public TournamentStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public Platform Platform { get; set; }
    public string PlatformName => Platform.ToString();
    public int MaxTeams { get; set; }
    public int RegisteredTeamsCount { get; set; }
    public DateTime? StartDate { get; set; }
    public decimal? PrizePool { get; set; }
    public string? BannerImageUrl { get; set; }
    public string? OrganizerName { get; set; }
    public DateTime CreatedAt { get; set; }
}
