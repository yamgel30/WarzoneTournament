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

    // Legacy only runs this check when creating brand new (not editing, not resubmitting) --
    // callers get true (already has one) back for isEdit/isResubmit without a query.
    public async Task<bool> MemberAlreadyHasAhaAsync(string memberId, bool isEdit, bool isResubmit, int ahaYear, bool atHome, CancellationToken cancellationToken = default)
    {
        if (isEdit || isResubmit)
        {
            return false;
        }

        try
        {
            using var connection = connectionFactory.CreateConnection();
            var claimClass = await ClaimClassLookup.GetByYearAsync(connection, ahaYear, cancellationToken);

            var command = new CommandDefinition(
                """
                SELECT ISNULL(COUNT(*),0) AS CountOfSubmitted
                FROM Claims WITH (NOLOCK)
                INNER JOIN vwMSV_Master WITH (NOLOCK) ON Claims.sPatientContract = vwMSV_Master.memberID
                INNER JOIN Claims_AHADetail WITH (NOLOCK) ON Claims_AHADetail.biClaimID = Claims.biClaimID
                WHERE Claims.iStatus > 1
                  AND ISNULL(Claims.RejectTypeId, 0) <> 1
                  AND Claims.nClaimClass = @ClaimClass
                  AND Claims.nClaimClassTag = 1
                  AND vwMSV_Master.useForPCPEligibility = 1
                  AND Claims.sPatientContract = @MemberId
                  AND Claims_AHADetail.AtHome = @AtHome
                """,
                new { ClaimClass = claimClass, MemberId = memberId, AtHome = atHome },
                cancellationToken: cancellationToken);

            var countOfSubmitted = await connection.ExecuteScalarAsync<int>(command);
            return countOfSubmitted > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check whether member {MemberId} already has an AHA for {AhaYear}", memberId, ahaYear);
            return false;
        }
    }
}
