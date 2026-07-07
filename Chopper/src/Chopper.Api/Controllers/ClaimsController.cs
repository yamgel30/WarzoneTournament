using Chopper.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Chopper.Api.Controllers;

[ApiController]
[Route("api/claims")]
public sealed class ClaimsController(IClaimsVerificationService claimsVerificationService) : ControllerBase
{
    [HttpGet("{claimId:long}/concurrency")]
    public async Task<IActionResult> ValidateConcurrency(long claimId, [FromQuery] long concurrencyId, CancellationToken cancellationToken)
    {
        var result = await claimsVerificationService.ValidateConcurrencyAsync(claimId, concurrencyId, cancellationToken);
        return Ok(new { result });
    }

    [HttpGet("tha-verification")]
    public async Task<IActionResult> VerifyMemberHasTha(
        [FromQuery] string memberId,
        [FromQuery] short claimClass,
        [FromQuery] long claimId,
        [FromQuery] bool atHome,
        CancellationToken cancellationToken)
    {
        var exists = await claimsVerificationService.MemberHasThaForYearAsync(memberId, claimClass, claimId, atHome, cancellationToken);
        return Ok(new { exists });
    }

    [HttpGet("form-exists")]
    public async Task<IActionResult> VerifyFormExists(
        [FromQuery] string memberId,
        [FromQuery] DateTime dateOfService,
        [FromQuery] short claimClass,
        [FromQuery] long? claimId,
        CancellationToken cancellationToken)
    {
        var exists = await claimsVerificationService.FormExistsForDateOfServiceAsync(memberId, dateOfService, claimClass, claimId, cancellationToken);
        return Ok(new { exists });
    }
}
