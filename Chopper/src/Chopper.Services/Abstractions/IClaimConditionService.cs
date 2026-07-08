using Chopper.Services.ClaimConditions;

namespace Chopper.Services.Abstractions;

public interface IClaimConditionService
{
    Task<IReadOnlyList<MemberConditionItem>> GetMemberActiveConditionAsync(string memberId, int? year, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MemberConditionItem>> GetMemberSuspiciousConditionAsync(string memberId, int? year, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClaimDiagnosisItem>> GetClaimDiagnosticListAsync(long claimId, CancellationToken cancellationToken = default);

    Task<string> GetRejectNotesAsync(long claimId, CancellationToken cancellationToken = default);

    Task<bool> SaveAhaDxHxSelectionAsync(long claimId, IReadOnlyList<DxHistorySelectionItem> items, CancellationToken cancellationToken = default);

    // Mirrors legacy's GetAHADxHxSelection, which is a no-op stub that always returns an empty
    // list -- kept for interface parity, not because there's real logic to preserve.
    Task<IReadOnlyList<DxHistorySelectionItem>> GetAhaDxHxSelectionAsync(long claimId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DxHistorySelectionItem>> GetMemberDxHistoryAsync(string memberId, CancellationToken cancellationToken = default);

    Task<bool> SaveSuspiciousConditionDxHxSelectionAsync(long claimId, IReadOnlyList<SuspiciousConditionSelectionItem> items, CancellationToken cancellationToken = default);

    Task<bool> SaveSuspiciousConditionDxHxSelectionV2Async(long claimId, IReadOnlyList<SuspiciousConditionSelectionItem> items, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IcdCode>> GetIcdLookupAsync(string searchText, DateTime serviceDate, int ahaYear, CancellationToken cancellationToken = default);

    Task<MemberClaimStatusResult> GetMemberClaimStatusAsync(string memberId, int claimClass, CancellationToken cancellationToken = default);
}
