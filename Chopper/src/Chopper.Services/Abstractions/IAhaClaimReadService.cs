using Chopper.Services.AhaClaims;

namespace Chopper.Services.Abstractions;

// The read-side counterpart of IAhaClaimService, mirroring legacy's GetAHA. GetAHA is one huge
// method built around a single stored procedure (uspGetAHA2) returning an 11-table result set --
// this interface is filled in section by section, the same incremental way SaveClaim was, starting
// with the form header (every other section depends on knowing the claim exists first).
public interface IAhaClaimReadService
{
    Task<AhaFormHeader?> GetFormHeaderAsync(long claimId, CancellationToken cancellationToken = default);
}
