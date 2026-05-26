using WarzoneTournament.Domain.Enums;

namespace WarzoneTournament.Application.DTOs.Evidence;

public class EvidenceDto
{
    public Guid Id { get; set; }
    public Guid MatchId { get; set; }
    public Guid SubmittedByTeamId { get; set; }
    public string? SubmittedByTeamName { get; set; }
    public Guid? SubmittedByPlayerId { get; set; }
    public string? SubmittedByPlayerUsername { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public EvidenceStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public EvidenceSourceType SourceType { get; set; }
    public string SourceTypeName => SourceType.ToString();
    public string? DiscordMessageId { get; set; }
    public string? OriginalFileName { get; set; }
    public long? FileSizeBytes { get; set; }
    public bool OcrProcessed { get; set; }
    public string? RejectionReason { get; set; }
    public OcrResultDto? OcrResult { get; set; }
    public List<EvidenceReviewDto> Reviews { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class EvidenceReviewDto
{
    public Guid Id { get; set; }
    public string ReviewedBy { get; set; } = string.Empty;
    public EvidenceStatus Decision { get; set; }
    public string? Notes { get; set; }
    public DateTime ReviewedAt { get; set; }
}
