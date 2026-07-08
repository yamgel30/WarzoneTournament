using Chopper.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Chopper.Api.Controllers;

[ApiController]
[Route("api/sub-projects")]
public sealed class SubProjectsController(IClaimsVerificationService claimsVerificationService) : ControllerBase
{
    [HttpGet("{projectName}/active")]
    public async Task<IActionResult> IsActive(string projectName, [FromQuery] short year, CancellationToken cancellationToken)
    {
        var isActive = await claimsVerificationService.IsSubProjectActiveAsync(projectName, year, cancellationToken);
        return Ok(new { isActive });
    }
}
