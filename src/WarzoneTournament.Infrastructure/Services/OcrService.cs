using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using WarzoneTournament.Application.Common.Interfaces;
using WarzoneTournament.Application.Common.Models;
using WarzoneTournament.Application.DTOs.Evidence;
using WarzoneTournament.Domain.Interfaces;

namespace WarzoneTournament.Infrastructure.Services;

public class OcrService : IOcrService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<OcrService> _logger;
    private readonly string _ocrProvider;

    public OcrService(IUnitOfWork uow, ILogger<OcrService> logger, IConfiguration config)
    {
        _uow = uow;
        _logger = logger;
        _ocrProvider = config["Ocr:Provider"] ?? "Tesseract";
    }

    public async Task<Result<OcrResultDto>> ProcessEvidenceImageAsync(Guid evidenceId, CancellationToken ct = default)
    {
        var evidence = await _uow.MatchEvidences.GetByIdAsync(evidenceId, ct);
        if (evidence is null)
            return Result.Failure<OcrResultDto>("Evidence not found.");

        var result = await ExtractTextFromImageAsync(evidence.ImageUrl, ct);
        if (result.IsFailure)
            return result;

        var ocrResult = new Domain.Entities.OCRExtractionResult
        {
            EvidenceId = evidenceId,
            RawText = result.Value.RawText,
            ExtractedPlacement = result.Value.ExtractedPlacement,
            ExtractedKills = result.Value.ExtractedKills,
            ExtractedTeamName = result.Value.ExtractedTeamName,
            ConfidenceScore = result.Value.ConfidenceScore,
            RequiresManualReview = result.Value.RequiresManualReview,
            ProcessedAt = DateTime.UtcNow,
            OcrProvider = _ocrProvider
        };

        await _uow.OCRExtractionResults.AddAsync(ocrResult, ct);
        evidence.OcrProcessed = true;
        _uow.MatchEvidences.Update(evidence);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("OCR processed for evidence {EvidenceId}, confidence: {Score}",
            evidenceId, result.Value.ConfidenceScore);

        return result;
    }

    public async Task<Result<OcrResultDto>> ExtractTextFromImageAsync(string imageUrl, CancellationToken ct = default)
    {
        try
        {
            // In production: use Tesseract or Azure Cognitive Services
            // For now, simulate OCR with pattern matching on a placeholder approach
            var rawText = await SimulateOcrAsync(imageUrl, ct);
            var extracted = ParseWarzoneScoreboard(rawText);

            return Result.Success(new OcrResultDto
            {
                RawText = rawText,
                ExtractedPlacement = extracted.Placement,
                ExtractedKills = extracted.Kills,
                ExtractedTeamName = extracted.TeamName,
                ConfidenceScore = extracted.Confidence,
                RequiresManualReview = extracted.Confidence < 0.75m,
                ProcessedAt = DateTime.UtcNow,
                OcrProvider = _ocrProvider
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OCR processing failed for {ImageUrl}", imageUrl);
            return Result.Success(new OcrResultDto
            {
                RawText = string.Empty,
                RequiresManualReview = true,
                ProcessingError = ex.Message,
                ConfidenceScore = 0,
                ProcessedAt = DateTime.UtcNow,
                OcrProvider = _ocrProvider
            });
        }
    }

    private static async Task<string> SimulateOcrAsync(string imageUrl, CancellationToken ct)
    {
        // Production: replace with actual Tesseract or Azure Vision call
        // Tesseract example:
        // using var engine = new TesseractEngine("tessdata", "eng", EngineMode.Default);
        // using var img = Pix.LoadFromFile(localPath);
        // using var page = engine.Process(img);
        // return page.GetText();
        await Task.Delay(100, ct);
        return "WARZONE SCOREBOARD\n#1 PLACEMENT\nKILLS: 5\nTEAM: EXAMPLE";
    }

    private static (int? Placement, int? Kills, string? TeamName, decimal Confidence) ParseWarzoneScoreboard(string text)
    {
        int? placement = null;
        int? kills = null;
        string? teamName = null;
        int matchCount = 0;
        const int totalPatterns = 3;

        // Placement patterns: "#1", "1st", "PLACEMENT: 1", "#01"
        var placementPatterns = new[]
        {
            @"#\s*(\d{1,3})\b",
            @"\b(\d{1,3})\s*(st|nd|rd|th)\b",
            @"placement[:\s]+(\d{1,3})",
            @"rank[:\s]+(\d{1,3})"
        };

        foreach (var pattern in placementPatterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Groups[1].Value, out int p) && p is >= 1 and <= 150)
            {
                placement = p;
                matchCount++;
                break;
            }
        }

        // Kill patterns
        var killPatterns = new[]
        {
            @"kills?[:\s]+(\d{1,3})",
            @"(\d{1,3})\s+kills?",
            @"\bk[:\s]*(\d{1,3})\b"
        };

        foreach (var pattern in killPatterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Groups[1].Value, out int k) && k is >= 0 and <= 50)
            {
                kills = k;
                matchCount++;
                break;
            }
        }

        // Team name
        var teamMatch = Regex.Match(text, @"team[:\s]+([A-Za-z0-9_\-\s]{2,30})", RegexOptions.IgnoreCase);
        if (teamMatch.Success)
        {
            teamName = teamMatch.Groups[1].Value.Trim();
            matchCount++;
        }

        var confidence = totalPatterns > 0 ? (decimal)matchCount / totalPatterns : 0;
        return (placement, kills, teamName, confidence);
    }
}
