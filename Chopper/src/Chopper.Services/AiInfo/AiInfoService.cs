using System.Data;
using Chopper.Services.Abstractions;
using Chopper.Services.Common;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Chopper.Services.AiInfo;

internal sealed class AiInfoService(
    ISqlConnectionFactory connectionFactory,
    ILogger<AiInfoService> logger) : IAiInfoService
{
    public async Task<bool> SaveTranscriptAsync(long claimId, string? contractId, string transcript, bool hasFillAha, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = connectionFactory.CreateConnection();
            var command = new CommandDefinition(
                "usp_AHAAIInfo_SaveInfo",
                new { ClaimID = claimId, ContractID = contractId, Transcript = transcript, HasFillAHA = hasFillAha },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save AI transcript for claim {ClaimId}", claimId);
            return false;
        }
    }

    public async Task<bool> SaveAudioAsync(long claimId, string? contractId, byte[] audioBytes, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = connectionFactory.CreateConnection();
            var command = new CommandDefinition(
                "usp_AHAAIInfo_SaveAudio",
                new { ClaimID = claimId, ContractID = contractId, AudioBytes = audioBytes },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save AI audio for claim {ClaimId}", claimId);
            return false;
        }
    }
}
