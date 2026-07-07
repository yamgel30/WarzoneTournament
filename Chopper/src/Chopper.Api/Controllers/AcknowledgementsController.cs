using Chopper.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Chopper.Api.Controllers;

[ApiController]
[Route("api/acknowledgements")]
public sealed class AcknowledgementsController(IAcknowledgementService acknowledgementService) : ControllerBase
{
    [HttpPost("welcome-letter")]
    public async Task<IActionResult> LogWelcomeLetter([FromQuery] string renderingNpi, [FromQuery] short year, CancellationToken cancellationToken)
    {
        var success = await acknowledgementService.LogWelcomeLetterAsync(renderingNpi, year, cancellationToken);
        return success ? Ok() : Problem(statusCode: StatusCodes.Status500InternalServerError);
    }

    [HttpGet("welcome-letter")]
    public async Task<IActionResult> HasWelcomeLetter([FromQuery] string renderingNpi, [FromQuery] short year, CancellationToken cancellationToken)
    {
        var exists = await acknowledgementService.HasWelcomeLetterAsync(renderingNpi, year, cancellationToken);
        return Ok(new { exists });
    }

    [HttpPost("functional-quadriplegia")]
    public async Task<IActionResult> LogFunctionalQuadriplegia([FromQuery] string renderingNpi, [FromQuery] short year, CancellationToken cancellationToken)
    {
        var success = await acknowledgementService.LogFunctionalQuadriplegiaMessageAsync(renderingNpi, year, cancellationToken);
        return success ? Ok() : Problem(statusCode: StatusCodes.Status500InternalServerError);
    }

    [HttpGet("functional-quadriplegia")]
    public async Task<IActionResult> HasFunctionalQuadriplegia([FromQuery] string renderingNpi, [FromQuery] short year, CancellationToken cancellationToken)
    {
        var exists = await acknowledgementService.HasFunctionalQuadriplegiaMessageAsync(renderingNpi, year, cancellationToken);
        return Ok(new { exists });
    }

    [HttpPost("inflammatory-polyarthritis")]
    public async Task<IActionResult> LogInflammatoryPolyarthritis([FromQuery] string renderingNpi, [FromQuery] short year, CancellationToken cancellationToken)
    {
        var success = await acknowledgementService.LogInflammatoryPolyarthritisMessageAsync(renderingNpi, year, cancellationToken);
        return success ? Ok() : Problem(statusCode: StatusCodes.Status500InternalServerError);
    }

    [HttpGet("inflammatory-polyarthritis")]
    public async Task<IActionResult> HasInflammatoryPolyarthritis([FromQuery] string renderingNpi, [FromQuery] short year, CancellationToken cancellationToken)
    {
        var exists = await acknowledgementService.HasInflammatoryPolyarthritisMessageAsync(renderingNpi, year, cancellationToken);
        return Ok(new { exists });
    }
}
