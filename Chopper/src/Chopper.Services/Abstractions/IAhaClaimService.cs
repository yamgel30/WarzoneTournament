using Chopper.Services.AhaClaims;

namespace Chopper.Services.Abstractions;

public interface IAhaClaimService
{
    Task<bool> SavePage1Async(long claimId, SavePage1Request request, CancellationToken cancellationToken = default);

    Task<bool> SavePage2Async(long claimId, SavePage2Request request, CancellationToken cancellationToken = default);
}
