using Chopper.Services.Abstractions;
using Chopper.Services.ClaimSearch;
using Microsoft.AspNetCore.Mvc;

namespace Chopper.Api.Controllers;

[ApiController]
[Route("api/claim-search")]
public sealed class ClaimSearchController(IClaimSearchService claimSearchService) : ControllerBase
{
    [HttpPost("pending")]
    public async Task<ActionResult<ClaimListResult>> GetPending([FromBody] ClaimSearchCriteria criteria, CancellationToken cancellationToken)
        => await claimSearchService.GetPendingAsync(criteria, cancellationToken);

    [HttpPost("pending-special-cover")]
    public async Task<ActionResult<ClaimListResult>> GetPendingSpecialCover([FromBody] ClaimSearchCriteria criteria, CancellationToken cancellationToken)
        => await claimSearchService.GetPendingSpecialCoverAsync(criteria, cancellationToken);

    [HttpPost("rejected")]
    public async Task<ActionResult<ClaimListResult>> GetRejected([FromBody] ClaimSearchCriteria criteria, CancellationToken cancellationToken)
        => await claimSearchService.GetRejectedAsync(criteria, cancellationToken);

    [HttpPost("submitted")]
    public async Task<ActionResult<ClaimListResult>> GetSubmitted([FromBody] ClaimSearchCriteria criteria, CancellationToken cancellationToken)
        => await claimSearchService.GetSubmittedAsync(criteria, cancellationToken);

    [HttpPost("in-progress")]
    public async Task<ActionResult<ClaimListResult>> GetInProgress([FromBody] ClaimSearchCriteria criteria, CancellationToken cancellationToken)
        => await claimSearchService.GetInProgressAsync(criteria, cancellationToken);

    [HttpPost("in-progress-2021")]
    public async Task<ActionResult<ClaimListResult>> GetInProgress2021([FromBody] ClaimSearchCriteria criteria, CancellationToken cancellationToken)
        => await claimSearchService.GetInProgress2021Async(criteria, cancellationToken);
}
