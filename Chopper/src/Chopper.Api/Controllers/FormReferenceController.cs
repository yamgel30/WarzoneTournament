using Chopper.Services.Abstractions;
using Chopper.Services.FormReference;
using Microsoft.AspNetCore.Mvc;

namespace Chopper.Api.Controllers;

[ApiController]
[Route("api/form-reference")]
public sealed class FormReferenceController(IFormReferenceService formReferenceService) : ControllerBase
{
    [HttpGet("aha-years/{ahaYear:int}")]
    public async Task<ActionResult<AhaYearInfo>> GetAhaYearItem(int ahaYear, CancellationToken cancellationToken)
        => Ok(await formReferenceService.GetAhaYearItemAsync(ahaYear, cancellationToken));

    [HttpGet("aha-headers")]
    public async Task<ActionResult<IReadOnlyList<AhaFormHeaderSummary>>> GetAhaHeaderList(
        [FromQuery] string? memberId,
        [FromQuery] string? renderingNpi,
        [FromQuery] string? billingNpi,
        [FromQuery] string? ipaName,
        [FromQuery] int formYear,
        CancellationToken cancellationToken)
        => Ok(await formReferenceService.GetAhaHeaderListAsync(memberId, renderingNpi, billingNpi, ipaName, formYear, cancellationToken));

    [HttpGet("history-present-illness-options")]
    public async Task<ActionResult<IReadOnlyList<HistoryPresentIllnessOption>>> GetHistoryPresentIllnessOptions(
        [FromQuery] int ahaPr = -1,
        [FromQuery] int ahaFl = -1,
        [FromQuery] int ghpAdult = -1,
        [FromQuery] int ghpPediatric = -1,
        [FromQuery] int isShort = -1,
        CancellationToken cancellationToken = default)
        => Ok(await formReferenceService.GetHistoryPresentIllnessOptionsAsync(ahaPr, ahaFl, ghpAdult, ghpPediatric, isShort, cancellationToken));
}
