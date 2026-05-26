using WarzoneTournament.Domain.Enums;

namespace WarzoneTournament.Application.DTOs.Tournament;

public class TournamentQueryDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public TournamentStatus? Status { get; set; }
    public TournamentType? Type { get; set; }
    public Platform? Platform { get; set; }
    public string? SearchTerm { get; set; }
    public bool? IsPrivate { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}
