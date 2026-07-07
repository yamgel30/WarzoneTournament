using Chopper.Services.Abstractions;
using Chopper.Services.AhaClaims;
using Microsoft.AspNetCore.Mvc;

namespace Chopper.Api.Controllers;

[ApiController]
[Route("api/aha-claims")]
public sealed class AhaClaimsController(IAhaClaimService ahaClaimService) : ControllerBase
{
    [HttpPut("{claimId:long}/pages/1")]
    public async Task<IActionResult> SavePage1(long claimId, [FromBody] SavePage1Request request, CancellationToken cancellationToken)
    {
        var success = await ahaClaimService.SavePage1Async(claimId, request, cancellationToken);
        return success ? Ok() : Problem(statusCode: StatusCodes.Status500InternalServerError);
    }

    [HttpPut("{claimId:long}/pages/3")]
    public async Task<IActionResult> SavePage3(long claimId, [FromBody] SavePage3Request request, CancellationToken cancellationToken)
    {
        var success = await ahaClaimService.SavePage3Async(claimId, request, cancellationToken);
        return success ? Ok() : Problem(statusCode: StatusCodes.Status500InternalServerError);
    }

    [HttpPut("{claimId:long}/pages/2")]
    public async Task<IActionResult> SavePage2(long claimId, [FromBody] SavePage2Request request, CancellationToken cancellationToken)
    {
        var success = await ahaClaimService.SavePage2Async(claimId, request, cancellationToken);
        return success ? Ok() : Problem(statusCode: StatusCodes.Status500InternalServerError);
    }
}
