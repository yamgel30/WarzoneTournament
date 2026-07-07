using Chopper.Services.Abstractions;
using Chopper.Services.AhaClaims;
using Microsoft.AspNetCore.Mvc;

namespace Chopper.Api.Controllers;

[ApiController]
[Route("api/aha-claims")]
public sealed class AhaClaimsController(IAhaClaimService ahaClaimService) : ControllerBase
{
    [HttpPut("{claimId:long}/pages/2")]
    public async Task<IActionResult> SavePage2(long claimId, [FromBody] SavePage2Request request, CancellationToken cancellationToken)
    {
        var success = await ahaClaimService.SavePage2Async(claimId, request, cancellationToken);
        return success ? Ok() : Problem(statusCode: StatusCodes.Status500InternalServerError);
    }
}
