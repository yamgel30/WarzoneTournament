namespace Chopper.Services.Abstractions;

public interface IClaimsVerificationService
{
    Task<int> ValidateConcurrencyAsync(long claimId, long concurrencyId, CancellationToken cancellationToken = default);

    Task<bool> MemberHasThaForYearAsync(string memberId, short claimClass, long claimId, bool atHome, CancellationToken cancellationToken = default);

    Task<bool> FormExistsForDateOfServiceAsync(string memberId, DateTime dateOfService, short claimClass, long? claimId, CancellationToken cancellationToken = default);

    Task<bool> IsSubProjectActiveAsync(string projectName, short year, CancellationToken cancellationToken = default);
}
