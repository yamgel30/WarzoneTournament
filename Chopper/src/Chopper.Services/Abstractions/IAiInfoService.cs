namespace Chopper.Services.Abstractions;

public interface IAiInfoService
{
    Task<bool> SaveTranscriptAsync(long claimId, string? contractId, string transcript, bool hasFillAha, CancellationToken cancellationToken = default);

    Task<bool> SaveAudioAsync(long claimId, string? contractId, byte[] audioBytes, CancellationToken cancellationToken = default);
}
