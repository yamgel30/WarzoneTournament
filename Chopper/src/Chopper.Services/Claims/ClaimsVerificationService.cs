using System.Data;
using Chopper.Services.Abstractions;
using Chopper.Services.Common;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Chopper.Services.Claims;

internal sealed class ClaimsVerificationService(
    ISqlConnectionFactory connectionFactory,
    ILogger<ClaimsVerificationService> logger) : IClaimsVerificationService
{
    public async Task<int> ValidateConcurrencyAsync(long claimId, long concurrencyId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = connectionFactory.CreateConnection();
            var command = new CommandDefinition(
                "uspClaim_ValidateConcurrencyID",
                new { claimID = claimId, concurrencyID = concurrencyId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            return await connection.ExecuteScalarAsync<int>(command);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to validate concurrency for claim {ClaimId}", claimId);
            return -1;
        }
    }

    public async Task<bool> MemberHasThaForYearAsync(string memberId, short claimClass, long claimId, bool atHome, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = connectionFactory.CreateConnection();
            var command = new CommandDefinition(
                "uspAHA_VerifyMemberHasTHAForYear",
                new { MemberID = memberId, ClaimClass = claimClass, ClaimID = claimId, AtHome = atHome },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            return await connection.ExecuteScalarAsync<bool>(command);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to verify THA for member {MemberId}", memberId);
            return false;
        }
    }

    public async Task<bool> FormExistsForDateOfServiceAsync(string memberId, DateTime dateOfService, short claimClass, long? claimId, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = connectionFactory.CreateConnection();
            var command = new CommandDefinition(
                "uspAHA_VerifyIfFormExistsForDOS",
                new { memberID = memberId, claimClass, DOS = dateOfService, claimID = claimId },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            return await connection.ExecuteScalarAsync<bool>(command);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to verify form existence for member {MemberId}", memberId);
            return false;
        }
    }

    public async Task<bool> IsSubProjectActiveAsync(string projectName, short year, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = connectionFactory.CreateConnection();
            var command = new CommandDefinition(
                "usp_AHAVerifySubProjectIsActive",
                new { projectName, Year = year },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            return await connection.ExecuteScalarAsync<bool>(command);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to verify sub-project {ProjectName} is active", projectName);
            return false;
        }
    }
}
