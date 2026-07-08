using Chopper.Services.Providers;

namespace Chopper.Services.Abstractions;

public interface IProviderService
{
    Task<IReadOnlyList<ProviderBillingItem>> GetBillingOfRenderingAsync(string renderingNpi, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProviderInfoItem>> GetProvidersInformationAsync(IReadOnlyList<string> renderingNpiList, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProviderInfoItem>> GetProvidersInformationFromBillingAsync(
        IReadOnlyList<string> billingNpiList, string? pcpNpi, int ahaYear, string? ipaName, CancellationToken cancellationToken = default);

    Task<RenderingNpiLookupResult> GetRenderingNpiOfMemberAsync(string memberId, IReadOnlyList<string> billingNpiList, CancellationToken cancellationToken = default);

    Task<RenderingNpiLookupResult> GetRenderingNpiOfMemberPraiAsync(string memberId, IReadOnlyList<string> billingNpiList, CancellationToken cancellationToken = default);

    Task<long> GetPendingClaimAsync(string memberId, string renderingNpi, string billingNpi, bool isShort, CancellationToken cancellationToken = default);

    Task<long> GetPendingClaim2023Async(string memberId, string renderingNpi, string billingNpi, bool atHome, bool isShort, CancellationToken cancellationToken = default);

    Task<MemberEligibilityResult> VerifyEligibilityAsync(
        string memberId, DateTime dateOfService, string renderingNpi, string billingNpi, string? payerId, CancellationToken cancellationToken = default);
}
