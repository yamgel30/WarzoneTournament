using WarzoneTournament.Domain.Common;
using WarzoneTournament.Domain.Enums;

namespace WarzoneTournament.Domain.Entities;

public class MatchEvidence : BaseEntity
{
    public Guid MatchId { get; set; }
    public Guid SubmittedByTeamId { get; set; }
    public Guid? SubmittedByPlayerId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public EvidenceStatus Status { get; set; } = EvidenceStatus.Pending;
    public EvidenceSourceType SourceType { get; set; } = EvidenceSourceType.ManualUpload;
    public string? DiscordMessageId { get; set; }
    public string? DiscordChannelId { get; set; }
    public string? OriginalFileName { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? MimeType { get; set; }
    public bool OcrProcessed { get; set; } = false;
    public string? RejectionReason { get; set; }

    public Match Match { get; set; } = null!;
    public Team SubmittedByTeam { get; set; } = null!;
    public Player? SubmittedByPlayer { get; set; }
    public ICollection<EvidenceReview> Reviews { get; set; } = new List<EvidenceReview>();
    public OCRExtractionResult? OcrResult { get; set; }
}
