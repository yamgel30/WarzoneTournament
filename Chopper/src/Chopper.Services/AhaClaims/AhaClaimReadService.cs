using System.Data;
using Chopper.Services.Abstractions;
using Chopper.Services.Common;
using Dapper;

namespace Chopper.Services.AhaClaims;

// Legacy's GetAHA logs and re-throws on failure rather than swallowing it (unlike SaveClaim's
// section savers) -- exceptions here are left to propagate to ASP.NET Core's normal error
// handling too, matching every other read-side service ported so far (ClaimSearchService,
// ProviderService, ClaimConditionService, FormReferenceService). A null return means the claim
// genuinely wasn't found, not that something went wrong.
internal sealed class AhaClaimReadService(ISqlConnectionFactory connectionFactory) : IAhaClaimReadService
{
    public async Task<AhaFormHeader?> GetFormHeaderAsync(long claimId, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var tables = await LoadAhaDataSetAsync(connection, claimId, cancellationToken);

        if (tables.Count == 0 || tables[0].Count == 0)
        {
            return null;
        }

        var row = tables[0][0];

        // Table 2, row 0, column sPayerID -- a different result set than the rest of the
        // header, exactly as legacy reads it.
        string? payerId = tables.Count > 2 && tables[2].Count > 0 ? (string?)tables[2][0].sPayerID : null;

        // MemberDob is read twice in legacy: once from birthDate, then unconditionally
        // overwritten by PatientBirth when that column is present. Both draw from the same
        // row here, so PatientBirth simply wins when set.
        DateTime? memberDob = GetDateOrNull(row, "birthDate");
        var patientBirth = GetDateOrNull(row, "PatientBirth");
        if (patientBirth.HasValue)
        {
            memberDob = patientBirth;
        }

        return new AhaFormHeader
        {
            Id = claimId,
            SubmittedDate = GetDateOrNull(row, "SubmittedDate") ?? DateTime.Now,
            Language = GetString(row, "AHALanguage"),
            BillingNpi = GetString(row, "BillingNPI"),
            DateOfVisit = GetDateOrNull(row, "ServiceDate"),
            HealthPlan = GetString(row, "PayerName"),
            MemberDob = memberDob,
            MemberGender = GetString(row, "genderID"),
            MemberId = GetString(row, "MemberID"),
            MemberName = GetString(row, "MemberName"),
            PayerId = payerId,
            ProviderName = GetString(row, "ProvName"),
            RenderingNpi = GetString(row, "RenderingNPI"),
            AtHome = GetBoolOrNull(row, "AtHome"),
            PlaceOfService = GetIntOrNull(row, "nPOS"),
            Status = GetIntOrNull(row, "iStatus"),
            ApprovedOrRejectedDate = GetDateOrNull(row, "dApproved"),
            MemberFirstName = GetString(row, "MemberFName"),
            MemberMiddleName = GetString(row, "MemberMName"),
            MemberLastName = GetString(row, "MemberLName"),
            RenderingName = GetString(row, "RenderingName"),
            BillingName = GetString(row, "BillingName"),
            IpaName = GetString(row, "IPAName"),
            AccompaniedBy = GetString(row, "AccompaniedBy"),
            TypeOfVisit = GetIntOrNull(row, "TypeOfVisit"),
            ConcurrencyId = GetLongOrNull(row, "ConcurrencyID") ?? 0,
            ClaimClassTag = GetIntOrNull(row, "nClaimClassTag"),
            MemberLanguage = GetString(row, "Member_Language"),
            MemberLanguageOther = GetString(row, "Member_Language_other"),
            Race = GetString(row, "Member_Race"),
            Ethnicity = GetString(row, "Member_Ethnicity"),
            Phone = GetString(row, "Member_Phone"),
            Email = GetString(row, "Member_EMail"),
            AdditionalHealthPlan = GetString(row, "Member_Additional_Health_Plan"),
            AdditionalHealthPlanOther = GetString(row, "Member_Additional_Health_Plan_Other"),
            SexualOrientation = GetString(row, "Member_Sexual_Orientation"),
            SexAtBirth = GetString(row, "Member_Sex_Birth"),
            Pronoun = GetString(row, "Member_Pronoun"),
            GenderIdentity = GetString(row, "Member_Gender_Identity"),
            SexualOrientationSomethingElse = GetString(row, "Member_Sexual_OrientationSomethingelse"),
            PronounOtherPronoun = GetString(row, "Member_PronounOther_Pronoun"),
            OtherRace = GetString(row, "Member_Other_Race"),
            GenderIdentityAdditionalGender = GetString(row, "Member_Gender_Identity_AdditionalGender"),
        };
    }

    // uspGetAHA2 returns an 11-table result set (DataSet in legacy); read once and keep every
    // table around so later batches can pull whichever ones they need without a second round trip.
    private static async Task<IReadOnlyList<IReadOnlyList<dynamic>>> LoadAhaDataSetAsync(IDbConnection connection, long claimId, CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(
            "uspGetAHA2",
            new { biClaimID = claimId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var tables = new List<IReadOnlyList<dynamic>>();
        while (!multi.IsConsumed)
        {
            var rows = (await multi.ReadAsync()).ToList();
            tables.Add(rows);
        }

        return tables;
    }

    // Column types for uspGetAHA2's ~300 columns aren't independently confirmed against a real SP
    // definition (not part of the SP data provided so far), so reads go through a tolerant
    // Convert.ToXxx rather than a strict typed access, mirroring the same reasoning used for the
    // claim list/search stored procedures.
    private static object? GetRawValue(IDictionary<string, object> row, string column)
        => row.TryGetValue(column, out var value) && value is not null and not DBNull ? value : null;

    private static string? GetString(IDictionary<string, object> row, string column) => GetRawValue(row, column)?.ToString();

    private static DateTime? GetDateOrNull(IDictionary<string, object> row, string column) => GetRawValue(row, column) is { } v ? Convert.ToDateTime(v) : null;

    private static int? GetIntOrNull(IDictionary<string, object> row, string column) => GetRawValue(row, column) is { } v ? Convert.ToInt32(v) : null;

    private static long? GetLongOrNull(IDictionary<string, object> row, string column) => GetRawValue(row, column) is { } v ? Convert.ToInt64(v) : null;

    private static bool? GetBoolOrNull(IDictionary<string, object> row, string column) => GetRawValue(row, column) is { } v ? Convert.ToBoolean(v) : null;
}
