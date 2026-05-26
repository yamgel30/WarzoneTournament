namespace WarzoneTournament.Application.DTOs.Evidence;

public class OcrResultDto
{
    public Guid EvidenceId { get; set; }
    public string RawText { get; set; } = string.Empty;
    public int? ExtractedPlacement { get; set; }
    public int? ExtractedKills { get; set; }
    public string? ExtractedTeamName { get; set; }
    public decimal ConfidenceScore { get; set; }
    public bool RequiresManualReview { get; set; }
    public string? ProcessingError { get; set; }
    public DateTime ProcessedAt { get; set; }
    public string OcrProvider { get; set; } = string.Empty;
}
