using Chopper.Services.Abstractions;
using Chopper.Services.ClaimConditions;
using Microsoft.AspNetCore.Mvc;

namespace Chopper.Api.Controllers;

[ApiController]
[Route("api/claim-conditions")]
public sealed class ClaimConditionsController(IClaimConditionService claimConditionService) : ControllerBase
{
    [HttpGet("members/{memberId}/active-conditions")]
    public async Task<ActionResult<IReadOnlyList<MemberConditionItem>>> GetMemberActiveConditions(string memberId, [FromQuery] int? year, CancellationToken cancellationToken)
        => Ok(await claimConditionService.GetMemberActiveConditionAsync(memberId, year, cancellationToken));

    [HttpGet("members/{memberId}/suspicious-conditions")]
    public async Task<ActionResult<IReadOnlyList<MemberConditionItem>>> GetMemberSuspiciousConditions(string memberId, [FromQuery] int? year, CancellationToken cancellationToken)
        => Ok(await claimConditionService.GetMemberSuspiciousConditionAsync(memberId, year, cancellationToken));

    [HttpGet("{claimId:long}/diagnoses")]
    public async Task<ActionResult<IReadOnlyList<ClaimDiagnosisItem>>> GetClaimDiagnosticList(long claimId, CancellationToken cancellationToken)
        => Ok(await claimConditionService.GetClaimDiagnosticListAsync(claimId, cancellationToken));

    [HttpGet("{claimId:long}/reject-notes")]
    public async Task<ActionResult<string>> GetRejectNotes(long claimId, CancellationToken cancellationToken)
        => Ok(await claimConditionService.GetRejectNotesAsync(claimId, cancellationToken));

    [HttpPut("{claimId:long}/dx-history-selection")]
    public async Task<IActionResult> SaveAhaDxHxSelection(long claimId, [FromBody] IReadOnlyList<DxHistorySelectionItem> items, CancellationToken cancellationToken)
    {
        var success = await claimConditionService.SaveAhaDxHxSelectionAsync(claimId, items, cancellationToken);
        return success ? Ok() : Problem(statusCode: StatusCodes.Status500InternalServerError);
    }

    [HttpGet("{claimId:long}/dx-history-selection")]
    public async Task<ActionResult<IReadOnlyList<DxHistorySelectionItem>>> GetAhaDxHxSelection(long claimId, CancellationToken cancellationToken)
        => Ok(await claimConditionService.GetAhaDxHxSelectionAsync(claimId, cancellationToken));

    [HttpGet("members/{memberId}/dx-history")]
    public async Task<ActionResult<IReadOnlyList<DxHistorySelectionItem>>> GetMemberDxHistory(string memberId, CancellationToken cancellationToken)
        => Ok(await claimConditionService.GetMemberDxHistoryAsync(memberId, cancellationToken));

    [HttpPut("{claimId:long}/suspicious-condition-dx-history-selection")]
    public async Task<IActionResult> SaveSuspiciousConditionDxHxSelection(long claimId, [FromBody] IReadOnlyList<SuspiciousConditionSelectionItem> items, CancellationToken cancellationToken)
    {
        var success = await claimConditionService.SaveSuspiciousConditionDxHxSelectionAsync(claimId, items, cancellationToken);
        return success ? Ok() : Problem(statusCode: StatusCodes.Status500InternalServerError);
    }

    [HttpPut("{claimId:long}/suspicious-condition-dx-history-selection/v2")]
    public async Task<IActionResult> SaveSuspiciousConditionDxHxSelectionV2(long claimId, [FromBody] IReadOnlyList<SuspiciousConditionSelectionItem> items, CancellationToken cancellationToken)
    {
        var success = await claimConditionService.SaveSuspiciousConditionDxHxSelectionV2Async(claimId, items, cancellationToken);
        return success ? Ok() : Problem(statusCode: StatusCodes.Status500InternalServerError);
    }

    [HttpGet("icd-lookup")]
    public async Task<ActionResult<IReadOnlyList<IcdCode>>> GetIcdLookup([FromQuery] string searchText, [FromQuery] DateTime serviceDate, [FromQuery] int ahaYear, CancellationToken cancellationToken)
        => Ok(await claimConditionService.GetIcdLookupAsync(searchText, serviceDate, ahaYear, cancellationToken));

    [HttpGet("members/{memberId}/status")]
    public async Task<ActionResult<MemberClaimStatusResult>> GetMemberClaimStatus(string memberId, [FromQuery] int claimClass, CancellationToken cancellationToken)
        => Ok(await claimConditionService.GetMemberClaimStatusAsync(memberId, claimClass, cancellationToken));
}
