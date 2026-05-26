using Microsoft.Extensions.Logging;
using WarzoneTournament.Application.Common.Interfaces;

namespace WarzoneTournament.Infrastructure.BackgroundJobs;

public class OcrBackgroundJob
{
    private readonly IOcrService _ocrService;
    private readonly ISignalRNotificationService _signalR;
    private readonly ILogger<OcrBackgroundJob> _logger;

    public OcrBackgroundJob(IOcrService ocrService, ISignalRNotificationService signalR,
        ILogger<OcrBackgroundJob> logger)
    {
        _ocrService = ocrService;
        _signalR = signalR;
        _logger = logger;
    }

    public async Task ProcessEvidenceOcrAsync(Guid evidenceId, Guid matchId)
    {
        _logger.LogInformation("Starting OCR processing for evidence {EvidenceId}", evidenceId);

        var result = await _ocrService.ProcessEvidenceImageAsync(evidenceId);

        if (result.IsSuccess)
        {
            await _signalR.NotifyEvidenceReviewedAsync(evidenceId,
                result.Value.RequiresManualReview ? "OcrRequiresReview" : "OcrCompleted");

            _logger.LogInformation("OCR completed for evidence {EvidenceId}. Confidence: {Score}, Requires manual review: {Review}",
                evidenceId, result.Value.ConfidenceScore, result.Value.RequiresManualReview);
        }
        else
        {
            _logger.LogError("OCR failed for evidence {EvidenceId}: {Error}", evidenceId, result.Error);
        }
    }
}
