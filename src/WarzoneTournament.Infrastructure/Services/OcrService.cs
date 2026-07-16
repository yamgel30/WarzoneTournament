using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Http;
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
    private readonly IHttpClientFactory _httpFactory;
    private readonly string _tessdataPath;
    private readonly string? _tesseractExe;
    private readonly bool _ocrEnabled;

    private static readonly string[] CommonWindowsPaths =
    [
        @"C:\Program Files\Tesseract-OCR\tesseract.exe",
        @"C:\Program Files (x86)\Tesseract-OCR\tesseract.exe"
    ];

    public OcrService(IUnitOfWork uow, ILogger<OcrService> logger, IConfiguration config, IHttpClientFactory httpFactory)
    {
        _uow = uow;
        _logger = logger;
        _httpFactory = httpFactory;
        _tessdataPath = config["Ocr:TesseractDataPath"] ?? "tessdata";

        // Resolve tesseract.exe: config override → common install paths → PATH
        var configured = config["Ocr:TesseractExePath"];
        if (!string.IsNullOrEmpty(configured) && File.Exists(configured))
            _tesseractExe = configured;
        else
            _tesseractExe = CommonWindowsPaths.FirstOrDefault(File.Exists);

        var engFile = Path.Combine(_tessdataPath, "eng.traineddata");
        _ocrEnabled = _tesseractExe is not null && File.Exists(engFile);

        if (_ocrEnabled)
            _logger.LogInformation("Tesseract OCR enabled. Exe: {Exe} | Tessdata: {Data}",
                _tesseractExe, Path.GetFullPath(_tessdataPath));
        else
            _logger.LogWarning(
                "Tesseract OCR not ready. Exe found: {ExeFound} | eng.traineddata found: {DataFound}. " +
                "Install Tesseract from https://github.com/UB-Mannheim/tesseract/wiki and place " +
                "eng.traineddata in '{DataPath}'. Optionally set Ocr:TesseractExePath in appsettings.",
                _tesseractExe is not null, File.Exists(engFile), _tessdataPath);
    }

    public async Task<Result<OcrResultDto>> ProcessEvidenceImageAsync(Guid evidenceId, CancellationToken ct = default)
    {
        var evidence = await _uow.MatchEvidences.GetByIdAsync(evidenceId, ct);
        if (evidence is null)
            return Result.Failure<OcrResultDto>("Evidence not found.");

        var result = await ExtractTextFromImageAsync(evidence.ImageUrl, ct);
        if (result.IsFailure) return result;

        var existing = await _uow.OCRExtractionResults.FirstOrDefaultAsync(
            o => o.EvidenceId == evidenceId, ct);

        if (existing is null)
        {
            await _uow.OCRExtractionResults.AddAsync(new Domain.Entities.OCRExtractionResult
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
                OcrProvider = result.Value.OcrProvider ?? "Tesseract"
            }, ct);
        }

        evidence.OcrProcessed = true;
        _uow.MatchEvidences.Update(evidence);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation(
            "OCR processed for evidence {Id}. Confidence: {Score}, ManualReview: {Review}",
            evidenceId, result.Value.ConfidenceScore, result.Value.RequiresManualReview);

        return result;
    }

    public async Task<Result<OcrResultDto>> ExtractTextFromImageAsync(string imageUrl, CancellationToken ct = default)
    {
        if (!_ocrEnabled)
            return Result.Success(Fallback());

        string? tempFile = null;
        try
        {
            string localPath;

            if (imageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                imageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                var http = _httpFactory.CreateClient();
                http.Timeout = TimeSpan.FromSeconds(30);
                var bytes = await http.GetByteArrayAsync(imageUrl, ct);
                tempFile = Path.Combine(Path.GetTempPath(), $"ocr_{Guid.NewGuid()}.png");
                await File.WriteAllBytesAsync(tempFile, bytes, ct);
                localPath = tempFile;
            }
            else
            {
                localPath = Path.Combine("wwwroot",
                    imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            }

            if (!File.Exists(localPath))
                return Result.Success(Fallback("Image file not found on server."));

            var absData = Path.GetFullPath(_tessdataPath);
            var rawText = await RunTesseractCliAsync(localPath, absData, ct);

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
                OcrProvider = "Tesseract"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OCR processing failed for {ImageUrl}", imageUrl);
            return Result.Success(Fallback(ex.Message));
        }
        finally
        {
            if (tempFile is not null && File.Exists(tempFile))
                try { File.Delete(tempFile); } catch { /* best-effort */ }
        }
    }

    private async Task<string> RunTesseractCliAsync(string imagePath, string tessdata, CancellationToken ct)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _tesseractExe!,
                // stdout = output text, --psm 6 = uniform block of text, --tessdata-dir = explicit path
                Arguments = $"\"{imagePath}\" stdout --psm 6 -l eng --tessdata-dir \"{tessdata}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();

        var textTask = process.StandardOutput.ReadToEndAsync(ct);
        var errTask  = process.StandardError.ReadToEndAsync(ct);

        await process.WaitForExitAsync(ct);

        var text  = await textTask;
        var error = await errTask;

        if (process.ExitCode != 0 && string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException(
                $"tesseract.exe exited with code {process.ExitCode}: {error.Trim()}");

        return text;
    }

    private OcrResultDto Fallback(string? error = null) => new()
    {
        RawText = string.Empty,
        RequiresManualReview = true,
        ConfidenceScore = 0,
        OcrProvider = "Fallback",
        ProcessingError = error,
        ProcessedAt = DateTime.UtcNow
    };

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

        var confidence = (decimal)matchCount / totalPatterns;
        return (placement, kills, teamName, confidence);
    }
}
