using Chopper.Services.FormReference;

namespace Chopper.Services.Abstractions;

public interface IFormReferenceService
{
    Task<AhaYearInfo> GetAhaYearItemAsync(int ahaYear, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AhaFormHeaderSummary>> GetAhaHeaderListAsync(
        string? memberId, string? renderingNpi, string? billingNpi, string? ipaName, int formYear, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HistoryPresentIllnessOption>> GetHistoryPresentIllnessOptionsAsync(
        int ahaPr, int ahaFl, int ghpAdult, int ghpPediatric, int isShort, CancellationToken cancellationToken = default);
}
