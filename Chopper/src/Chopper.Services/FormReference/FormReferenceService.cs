using System.Data;
using Chopper.Services.Abstractions;
using Chopper.Services.Common;
using Dapper;

namespace Chopper.Services.FormReference;

internal sealed class FormReferenceService(ISqlConnectionFactory connectionFactory) : IFormReferenceService
{
    public async Task<AhaYearInfo> GetAhaYearItemAsync(int ahaYear, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "uspAHAYear_GetAHAYearItem",
            new { Year = ahaYear },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var row = (await connection.QueryAsync(command)).FirstOrDefault();
        if (row is null)
        {
            return new AhaYearInfo { Found = false, Year = ahaYear };
        }

        return new AhaYearInfo
        {
            Found = true,
            Year = ahaYear,
            ClaimClass = Convert.ToInt32((object)row.ClaimClass),
            IsReadOnly = Convert.ToBoolean((object)row.ReadOnly),
            ReadOnlyBy = (string?)row.ReadOnlyBy,
            ReadOnlyDate = row.ReadOnlyDateTime,
        };
    }

    // Legacy's clinicOrHeader flag on the private GetMemberHeaderList helper is dead: both branches
    // of the commented-out stored procedure choice resolved to the same "uspMemberClinicalFormAndHeaderForm"
    // call, so it's not exposed here.
    public async Task<IReadOnlyList<AhaFormHeaderSummary>> GetAhaHeaderListAsync(
        string? memberId, string? renderingNpi, string? billingNpi, string? ipaName, int formYear, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var claimClass = await ClaimClassLookup.GetByYearAsync(connection, formYear, cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("FormYear", formYear);
        parameters.Add("ClaimClass", claimClass);
        if (!string.IsNullOrEmpty(memberId))
        {
            parameters.Add("MemberID", memberId);
        }

        if (!string.IsNullOrEmpty(billingNpi))
        {
            parameters.Add("BillingNPI", billingNpi);
        }

        if (!string.IsNullOrEmpty(renderingNpi))
        {
            parameters.Add("RenderingNPI", renderingNpi);
        }

        if (!string.IsNullOrEmpty(ipaName))
        {
            parameters.Add("IPAName", ipaName);
        }

        var command = new CommandDefinition(
            "uspMemberClinicalFormAndHeaderForm",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync(command);
        return rows
            .Select(r => new AhaFormHeaderSummary
            {
                BillingNpi = (string?)r.billingNPI,
                RenderingNpi = (string?)r.renderingNPI,
                MemberId = (string?)r.memberID,
                MemberName = (string?)r.memberName,
                MemberFirstName = (string?)r.MemberFirstName,
                MemberMiddleName = (string?)r.MemberMiddleName,
                MemberLastName = (string?)r.MemberLastName,
                MemberDob = r.MemberBirthDate,
                MemberGender = (string?)r.MemberGender,
                PayerId = (string?)r.MemberPayerID,
                RenderingName = (string?)r.ProvName,
                ProviderName = (string?)r.ProvName,
                ProviderPostalCity = (string?)r.ProviderPostalCity,
                ProviderStreetCity = (string?)r.ProviderStreetCity,
                IpaName = (string?)r.IPAName,
                Status = 2,
            })
            .ToList();
    }

    public async Task<IReadOnlyList<HistoryPresentIllnessOption>> GetHistoryPresentIllnessOptionsAsync(
        int ahaPr, int ahaFl, int ghpAdult, int ghpPediatric, int isShort, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        if (ahaPr >= 0)
        {
            parameters.Add("AHAPR", ahaPr);
        }

        if (ahaFl >= 0)
        {
            parameters.Add("AHAFL", ahaFl);
        }

        if (ghpAdult >= 0)
        {
            parameters.Add("GHPAdult", ghpAdult);
        }

        if (ghpPediatric >= 0)
        {
            parameters.Add("GHPPediatric", ghpPediatric);
        }

        if (isShort >= 0)
        {
            parameters.Add("IsShort", isShort);
        }

        var command = new CommandDefinition(
            "uspHistoryPresentIllnessOptions_GetByFormType",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync(command);
        return rows
            .Select(r => new HistoryPresentIllnessOption
            {
                Id = Convert.ToInt32((object)r.HistoryPresentIllnessID),
                TextToShow = (string?)r.TextToShow,
                TextToShowEn = (string?)r.TextToShowEn,
            })
            .ToList();
    }
}
