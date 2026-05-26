using WarzoneTournament.Domain.Common;

namespace WarzoneTournament.Domain.Entities;

public class OCRExtractionResult : BaseEntity
{
    public Guid EvidenceId { get; set; }
    public string RawText { get; set; } = string.Empty;
    public int? ExtractedPlacement { get; set; }
    public int? ExtractedKills { get; set; }
    public string? ExtractedTeamName { get; set; }
    public decimal ConfidenceScore { get; set; } = 0;
    public bool RequiresManualReview { get; set; } = true;
    public string? ProcessingError { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public string OcrProvider { get; set; } = "Tesseract";

    public MatchEvidence Evidence { get; set; } = null!;
}
