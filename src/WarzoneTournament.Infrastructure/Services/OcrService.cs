using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.RegularExpressions;
using Tesseract;
using WarzoneTournament.Application.Common.Interfaces;
using WarzoneTournament.Application.Common.Models;
using WarzoneTournament.Application.DTOs.Evidence;
using WarzoneTournament.Domain.Interfaces;

namespace WarzoneTournament.Infrastructure.Services;

public class OcrService : IOcrService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<OcrService> _logger;
    private readonly IHttpClientFactory _httpFactory;
    private readonly string _ocrProvider;
    private readonly string _tessdataPath;
    private readonly bool _ocrEnabled;

    public OcrService(IUnitOfWork uow, ILogger<OcrService> logger, IConfiguration config, IHttpClientFactory httpFactory)
    {
        _uow = uow;
        _logger = logger;
        _httpFactory = httpFactory;
        _ocrProvider = config["Ocr:Provider"] ?? "Tesseract";
        _tessdataPath = config["Ocr:TesseractDataPath"] ?? "tessdata";

        var engFile = Path.Combine(_tessdataPath, "eng.traineddata");
        _ocrEnabled = Directory.Exists(_tessdataPath) && File.Exists(engFile);

        if (_ocrEnabled)
            _logger.LogInformation("Tesseract OCR enabled. Tessdata: {Path}", Path.GetFullPath(_tessdataPath));
        else
            _logger.LogWarning(
                "Tesseract OCR not configured — evidence will go to manual review. " +
                "To enable: (1) install Tesseract, (2) place eng.traineddata in '{Path}', " +
                "(3) set Ocr:TesseractDataPath in appsettings.json if using a different folder.",
                _tessdataPath);
    }

    public async Task<Result<OcrResultDto>> ProcessEvidenceImageAsync(Guid evidenceId, CancellationToken ct = default)
    {
        var evidence = await _uow.MatchEvidences.GetByIdAsync(evidenceId, ct);
        if (evidence is null)
            return Result.Failure<OcrResultDto>("Evidence not found.");

        var result = await ExtractTextFromImageAsync(evidence.ImageUrl, ct);
        if (result.IsFailure) return result;

        var ocrResult = new Domain.Entities.OCRExtractionResult
        {
            EvidenceId = evidenceId,
            RawText = result.Value.RawText ?? string.Empty,
            ExtractedPlacement = result.Value.ExtractedPlacement,
            ExtractedKills = result.Value.ExtractedKills,
            ExtractedTeamName = result.Value.ExtractedTeamName,
            ConfidenceScore = result.Value.ConfidenceScore,
            RequiresManualReview = result.Value.RequiresManualReview,
            ProcessingError = result.Value.ProcessingError,
            ProcessedAt = DateTime.UtcNow,
            OcrProvider = result.Value.OcrProvider ?? _ocrProvider
        };

        await _uow.OCRExtractionResults.AddAsync(ocrResult, ct);
        evidence.OcrProcessed = true;
        _uow.MatchEvidences.Update(evidence);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("OCR processed for evidence {EvidenceId}. Provider: {Provider}, Confidence: {Score}, ManualReview: {Review}",
            evidenceId, ocrResult.OcrProvider, ocrResult.ConfidenceScore, ocrResult.RequiresManualReview);

        return result;
    }

    public async Task<Result<OcrResultDto>> ExtractTextFromImageAsync(string imageUrl, CancellationToken ct = default)
    {
        // If tessdata not found → immediate fallback to manual review (no error)
        if (!_ocrEnabled)
        {
            return Result.Success(new OcrResultDto
            {
                RawText = string.Empty,
                RequiresManualReview = true,
                ConfidenceScore = 0,
                OcrProvider = "Fallback",
                ProcessedAt = DateTime.UtcNow
            });
        }

        string? tempFile = null;
        try
        {
            string localPath;

            if (imageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                imageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                // Download remote image (Discord CDN, etc.) to a temp file for OCR
                var http = _httpFactory.CreateClient();
                http.Timeout = TimeSpan.FromSeconds(30);
                var bytes = await http.GetByteArrayAsync(imageUrl, ct);
                tempFile = Path.Combine(Path.GetTempPath(), $"ocr_{Guid.NewGuid()}.jpg");
                await File.WriteAllBytesAsync(tempFile, bytes, ct);
                localPath = tempFile;
            }
            else
            {
                localPath = Path.Combine("wwwroot", imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            }

            if (!File.Exists(localPath))
            {
                return Result.Success(new OcrResultDto
                {
                    RequiresManualReview = true,
                    ConfidenceScore = 0,
                    ProcessingError = "Image file not found on server.",
                    OcrProvider = _ocrProvider,
                    ProcessedAt = DateTime.UtcNow
                });
            }

            // Run Tesseract in a thread-pool thread — TesseractEngine is synchronous and CPU-bound
            var rawText = await Task.Run(() =>
            {
                using var engine = new TesseractEngine(_tessdataPath, "eng", EngineMode.Default);
                using var img = Pix.LoadFromFile(localPath);
                using var page = engine.Process(img);
                return page.GetText();
            }, ct);

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
                ConfidenceScore = 0,
                ProcessingError = ex.Message,
                ProcessedAt = DateTime.UtcNow,
                OcrProvider = _ocrProvider
            });
        }
        finally
        {
            if (tempFile is not null && File.Exists(tempFile))
                try { File.Delete(tempFile); } catch { /* best-effort cleanup */ }
        }
    }

    private static (int? Placement, int? Kills, string? TeamName, decimal Confidence) ParseWarzoneScoreboard(string text)
    {
        int? placement = null;
        int? kills = null;
        string? teamName = null;
        int matchCount = 0;
        const int totalPatterns = 3;

        var placementPatterns = new[]
        {
            @"#\s*(\d{1,3})\b",
            @"\b(\d{1,3})\s*(st|nd|rd|th)\b",
            @"placement[:\s]+(\d{1,3})",
            @"rank[:\s]+(\d{1,3})"
        };

        foreach (var pattern in placementPatterns)
        {
            var m = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (m.Success && int.TryParse(m.Groups[1].Value, out int p) && p is >= 1 and <= 150)
            {
                placement = p;
                matchCount++;
                break;
            }
        }

        var killPatterns = new[]
        {
            @"kills?[:\s]+(\d{1,3})",
            @"(\d{1,3})\s+kills?",
            @"\bk[:\s]*(\d{1,3})\b"
        };

        foreach (var pattern in killPatterns)
        {
            var m = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (m.Success && int.TryParse(m.Groups[1].Value, out int k) && k is >= 0 and <= 50)
            {
                kills = k;
                matchCount++;
                break;
            }
        }

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
