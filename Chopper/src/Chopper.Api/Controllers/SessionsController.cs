using Chopper.Services.Abstractions;
using Chopper.Services.Sessions;
using Microsoft.AspNetCore.Mvc;

namespace Chopper.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SessionsController(ISessionService sessionService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PortalSession session, CancellationToken cancellationToken)
    {
        var portalSessionId = await sessionService.LogSessionAsync(session, cancellationToken);
        return Ok(new { portalSessionId });
    }
}
