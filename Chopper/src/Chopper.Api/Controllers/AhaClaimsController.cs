using Chopper.Services.Abstractions;
using Chopper.Services.AhaClaims;
using Microsoft.AspNetCore.Mvc;

namespace Chopper.Api.Controllers;

[ApiController]
[Route("api/aha-claims")]
public sealed class AhaClaimsController(IAhaClaimService ahaClaimService, IAhaClaimReadService ahaClaimReadService) : ControllerBase
{
    [HttpGet("{claimId:long}/header")]
    public async Task<ActionResult<AhaFormHeader>> GetFormHeader(long claimId, CancellationToken cancellationToken)
    {
        var header = await ahaClaimReadService.GetFormHeaderAsync(claimId, cancellationToken);
        return header is null ? NotFound() : Ok(header);
    }

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

    [HttpPut("{claimId:long}/pages/4")]
    public async Task<IActionResult> SavePage4(long claimId, [FromBody] SavePage4Request request, CancellationToken cancellationToken)
    {
        var success = await ahaClaimService.SavePage4Async(claimId, request, cancellationToken);
        return success ? Ok() : Problem(statusCode: StatusCodes.Status500InternalServerError);
    }
}
