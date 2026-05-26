using WarzoneTournament.Application.Common.Models;
using WarzoneTournament.Application.DTOs.Evidence;

namespace WarzoneTournament.Application.Common.Interfaces;

public interface IOcrService
{
    Task<Result<OcrResultDto>> ProcessEvidenceImageAsync(Guid evidenceId, CancellationToken ct = default);
    Task<Result<OcrResultDto>> ExtractTextFromImageAsync(string imageUrl, CancellationToken ct = default);
}
