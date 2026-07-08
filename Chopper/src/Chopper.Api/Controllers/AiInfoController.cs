using Chopper.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Chopper.Api.Controllers;

[ApiController]
[Route("api/ai-info")]
public sealed class AiInfoController(IAiInfoService aiInfoService) : ControllerBase
{
    public sealed record SaveTranscriptRequest(long ClaimId, string? ContractId, string Transcript, bool HasFillAha);

    public sealed record SaveAudioRequest(long ClaimId, string? ContractId, byte[] AudioBytes);

    [HttpPost("transcript")]
    public async Task<IActionResult> SaveTranscript([FromBody] SaveTranscriptRequest request, CancellationToken cancellationToken)
    {
        var success = await aiInfoService.SaveTranscriptAsync(request.ClaimId, request.ContractId, request.Transcript, request.HasFillAha, cancellationToken);
        return success ? Ok() : Problem(statusCode: StatusCodes.Status500InternalServerError);
    }

    [HttpPost("audio")]
    public async Task<IActionResult> SaveAudio([FromBody] SaveAudioRequest request, CancellationToken cancellationToken)
    {
        var success = await aiInfoService.SaveAudioAsync(request.ClaimId, request.ContractId, request.AudioBytes, cancellationToken);
        return success ? Ok() : Problem(statusCode: StatusCodes.Status500InternalServerError);
    }
}
