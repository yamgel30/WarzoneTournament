using Chopper.Services.Abstractions;
using Chopper.Services.Providers;
using Microsoft.AspNetCore.Mvc;

namespace Chopper.Api.Controllers;

[ApiController]
[Route("api/providers")]
public sealed class ProvidersController(IProviderService providerService) : ControllerBase
{
    [HttpGet("{renderingNpi}/billing")]
    public async Task<ActionResult<IReadOnlyList<ProviderBillingItem>>> GetBillingOfRendering(string renderingNpi, CancellationToken cancellationToken)
        => Ok(await providerService.GetBillingOfRenderingAsync(renderingNpi, cancellationToken));

    [HttpPost("information")]
    public async Task<ActionResult<IReadOnlyList<ProviderInfoItem>>> GetProvidersInformation([FromBody] IReadOnlyList<string> renderingNpiList, CancellationToken cancellationToken)
        => Ok(await providerService.GetProvidersInformationAsync(renderingNpiList, cancellationToken));

    public sealed record ProvidersFromBillingRequest(IReadOnlyList<string> BillingNpiList, string? PcpNpi, int AhaYear, string? IpaName);

    [HttpPost("information-from-billing")]
    public async Task<ActionResult<IReadOnlyList<ProviderInfoItem>>> GetProvidersInformationFromBilling([FromBody] ProvidersFromBillingRequest request, CancellationToken cancellationToken)
        => Ok(await providerService.GetProvidersInformationFromBillingAsync(request.BillingNpiList, request.PcpNpi, request.AhaYear, request.IpaName, cancellationToken));

    [HttpPost("{memberId}/rendering-npi")]
    public async Task<ActionResult<RenderingNpiLookupResult>> GetRenderingNpiOfMember(string memberId, [FromBody] IReadOnlyList<string> billingNpiList, CancellationToken cancellationToken)
        => Ok(await providerService.GetRenderingNpiOfMemberAsync(memberId, billingNpiList, cancellationToken));

    [HttpPost("{memberId}/rendering-npi-prai")]
    public async Task<ActionResult<RenderingNpiLookupResult>> GetRenderingNpiOfMemberPrai(string memberId, [FromBody] IReadOnlyList<string> billingNpiList, CancellationToken cancellationToken)
        => Ok(await providerService.GetRenderingNpiOfMemberPraiAsync(memberId, billingNpiList, cancellationToken));

    [HttpGet("pending-claim")]
    public async Task<ActionResult<long>> GetPendingClaim(
        [FromQuery] string memberId, [FromQuery] string renderingNpi, [FromQuery] string billingNpi, [FromQuery] bool isShort, CancellationToken cancellationToken)
        => Ok(await providerService.GetPendingClaimAsync(memberId, renderingNpi, billingNpi, isShort, cancellationToken));

    [HttpGet("pending-claim-2023")]
    public async Task<ActionResult<long>> GetPendingClaim2023(
        [FromQuery] string memberId, [FromQuery] string renderingNpi, [FromQuery] string billingNpi, [FromQuery] bool atHome, [FromQuery] bool isShort, CancellationToken cancellationToken)
        => Ok(await providerService.GetPendingClaim2023Async(memberId, renderingNpi, billingNpi, atHome, isShort, cancellationToken));

    [HttpGet("eligibility")]
    public async Task<ActionResult<MemberEligibilityResult>> VerifyEligibility(
        [FromQuery] string memberId,
        [FromQuery] DateTime dateOfService,
        [FromQuery] string renderingNpi,
        [FromQuery] string billingNpi,
        [FromQuery] string? payerId,
        CancellationToken cancellationToken)
    {
        // Legacy returns a specific "required information" error when billingNpi is absent,
        // without ever querying -- enforced here as a plain validation error instead.
        if (string.IsNullOrEmpty(billingNpi))
        {
            return BadRequest("billingNpi is required.");
        }

        return Ok(await providerService.VerifyEligibilityAsync(memberId, dateOfService, renderingNpi, billingNpi, payerId, cancellationToken));
    }
}
