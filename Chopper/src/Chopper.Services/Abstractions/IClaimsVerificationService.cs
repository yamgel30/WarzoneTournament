namespace Chopper.Services.Abstractions;

public interface IClaimsVerificationService
{
    Task<int> ValidateConcurrencyAsync(long claimId, long concurrencyId, CancellationToken cancellationToken = default);

    Task<bool> MemberHasThaForYearAsync(string memberId, short claimClass, long claimId, bool atHome, CancellationToken cancellationToken = default);

    Task<bool> FormExistsForDateOfServiceAsync(string memberId, DateTime dateOfService, short claimClass, long? claimId, CancellationToken cancellationToken = default);

    Task<bool> IsSubProjectActiveAsync(string projectName, short year, CancellationToken cancellationToken = default);

    Task<bool> MemberAlreadyHasAhaAsync(string memberId, bool isEdit, bool isResubmit, int ahaYear, bool atHome, CancellationToken cancellationToken = default);

    Task<bool> MemberHasAhaForYearAsync(string memberId, int year, string renderingNpi, int claimClassTag = 1, CancellationToken cancellationToken = default);

    Task<bool> MemberHasAhaForYearV2Async(string memberId, int year, string renderingNpi, bool atHome, int claimClassTag = 1, CancellationToken cancellationToken = default);
}
