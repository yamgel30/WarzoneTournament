using WarzoneTournament.Application.Common.Models;
using WarzoneTournament.Application.DTOs.Evidence;

namespace WarzoneTournament.Application.Common.Interfaces;

public interface IEvidenceService
{
    Task<Result<EvidenceDto>> SubmitEvidenceAsync(SubmitEvidenceDto dto, CancellationToken ct = default);
    Task<Result<EvidenceDto>> GetEvidenceByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<IReadOnlyList<EvidenceDto>>> GetEvidenceByMatchAsync(Guid matchId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<EvidenceDto>>> GetPendingEvidenceAsync(CancellationToken ct = default);
    Task<Result<EvidenceDto>> ApproveEvidenceAsync(Guid id, string reviewedBy, string? notes = null, CancellationToken ct = default);
    Task<Result<EvidenceDto>> RejectEvidenceAsync(Guid id, string reviewedBy, string reason, CancellationToken ct = default);
    Task<Result<EvidenceDto>> SubmitEvidenceFromDiscordAsync(DiscordEvidenceDto dto, CancellationToken ct = default);
}
