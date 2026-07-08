using Chopper.Services.ClaimSearch;

namespace Chopper.Services.Abstractions;

public interface IClaimSearchService
{
    Task<ClaimListResult> GetPendingAsync(ClaimSearchCriteria criteria, CancellationToken cancellationToken = default);

    Task<ClaimListResult> GetPendingSpecialCoverAsync(ClaimSearchCriteria criteria, CancellationToken cancellationToken = default);

    Task<ClaimListResult> GetRejectedAsync(ClaimSearchCriteria criteria, CancellationToken cancellationToken = default);

    Task<ClaimListResult> GetSubmittedAsync(ClaimSearchCriteria criteria, CancellationToken cancellationToken = default);

    Task<ClaimListResult> GetInProgressAsync(ClaimSearchCriteria criteria, CancellationToken cancellationToken = default);

    Task<ClaimListResult> GetInProgress2021Async(ClaimSearchCriteria criteria, CancellationToken cancellationToken = default);
}
