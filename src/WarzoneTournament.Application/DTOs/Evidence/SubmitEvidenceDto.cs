using WarzoneTournament.Domain.Enums;

namespace WarzoneTournament.Application.DTOs.Evidence;

public class SubmitEvidenceDto
{
    public Guid MatchId { get; set; }
    public Guid SubmittedByTeamId { get; set; }
    public Guid? SubmittedByPlayerId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? OriginalFileName { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? MimeType { get; set; }
    public EvidenceSourceType SourceType { get; set; } = EvidenceSourceType.ManualUpload;
}
