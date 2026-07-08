using System.Data;
using System.Data.Common;
using Chopper.Services.Abstractions;
using Chopper.Services.Common;
using Dapper;

namespace Chopper.Services.ClaimSearch;

internal sealed class ClaimSearchService(ISqlConnectionFactory connectionFactory, AhaSearchOptions options) : IClaimSearchService
{
    public async Task<ClaimListResult> GetPendingAsync(ClaimSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();

        var ahaYear = criteria.AhaYear == 0 ? options.DefaultYear : criteria.AhaYear;
        var claimClass = await ClaimClassLookup.GetByYearAsync(connection, ahaYear, cancellationToken);

        var parameters = BuildBaseParameters(criteria, ahaYear, claimClass);
        if (criteria.AtHome)
        {
            parameters.Add("AtHome", true);
        }

        var command = new CommandDefinition("uspAHA_GetPending", parameters, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
        return await ExecuteListAsync(connection, command, MapPendingItem);
    }

    public async Task<ClaimListResult> GetPendingSpecialCoverAsync(ClaimSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();

        // Legacy always overrides these two, ignoring whatever the caller sent.
        var ahaYear = options.DefaultYear;
        var claimClass = options.DefaultClaimClass;

        var parameters = BuildBaseParameters(criteria, ahaYear, claimClass);

        var command = new CommandDefinition("uspAHA_GetPendingSpecialCover", parameters, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
        return await ExecuteListAsync(connection, command, MapPendingItem);
    }

    public async Task<ClaimListResult> GetRejectedAsync(ClaimSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();

        // Legacy always overrides these three, ignoring whatever the caller sent.
        var ahaYear = options.DefaultYear;
        var claimClass = options.DefaultClaimClass;

        var parameters = BuildBaseParameters(criteria, ahaYear, claimClass);
        parameters.Add("AtHome", criteria.AtHome);
        parameters.Add("MaxDayToResubmit", options.MaxDayToResubmit);

        var command = new CommandDefinition("uspAHA_GetReject", parameters, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
        return await ExecuteListAsync(connection, command, MapRejectedItem);
    }

    public async Task<ClaimListResult> GetSubmittedAsync(ClaimSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();

        var ahaYear = criteria.AhaYear == 0 ? options.DefaultYear : criteria.AhaYear;
        var claimClass = await ClaimClassLookup.GetByYearAsync(connection, ahaYear, cancellationToken);

        var parameters = BuildBaseParameters(criteria, ahaYear, claimClass);
        parameters.Add("AtHome", criteria.AtHome);
        if (criteria.ClaimClassTag > 0)
        {
            parameters.Add("nClaimClassTag", criteria.ClaimClassTag);
        }

        var command = new CommandDefinition("uspAHA_GetSubmitted", parameters, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
        return await ExecuteListAsync(connection, command, MapSubmittedItem);
    }

    public async Task<ClaimListResult> GetInProgressAsync(ClaimSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();

        // Unlike the other list endpoints, legacy never derives or overrides ClaimClass here --
        // whatever the caller sent in criteria.ClaimClass goes straight to the SP.
        var ahaYear = criteria.AhaYear == 0 ? options.DefaultYear : criteria.AhaYear;

        var parameters = BuildBaseParameters(criteria, ahaYear, criteria.ClaimClass);
        parameters.Add("AtHome", criteria.AtHome);
        if (!string.IsNullOrEmpty(criteria.FormName) && !string.Equals(criteria.FormName, "all", StringComparison.OrdinalIgnoreCase))
        {
            parameters.Add("FormName", criteria.FormName);
        }

        var command = new CommandDefinition("uspAHA_GetInProgress", parameters, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
        return await ExecuteListAsync(connection, command, MapInProgressItem);
    }

    public async Task<ClaimListResult> GetInProgress2021Async(ClaimSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();

        var ahaYear = criteria.AhaYear == 0 ? options.DefaultYear : criteria.AhaYear;

        var parameters = BuildBaseParameters(criteria, ahaYear, criteria.ClaimClass);
        parameters.Add("AtHome", criteria.AtHome);
        if (!string.IsNullOrEmpty(criteria.FormName))
        {
            parameters.Add("FormName", criteria.FormName);
        }

        var command = new CommandDefinition("uspAHA_GetInProgress2021", parameters, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
        return await ExecuteListAsync(connection, command, MapInProgressItem);
    }

    private static DynamicParameters BuildBaseParameters(ClaimSearchCriteria criteria, int ahaYear, int claimClass)
    {
        var parameters = new DynamicParameters();
        parameters.Add("RenderingNPIs", BuildNpiTable(criteria.RenderingNpiList).AsTableValuedParameter("Providers"));
        parameters.Add("BillingNPIs", BuildNpiTable(criteria.BillingNpiList).AsTableValuedParameter("Providers"));

        if (!string.IsNullOrEmpty(criteria.DateFrom))
        {
            parameters.Add("DateFrom", criteria.DateFrom);
        }

        if (!string.IsNullOrEmpty(criteria.DateTo))
        {
            parameters.Add("DateTo", criteria.DateTo);
        }

        if (!string.IsNullOrEmpty(criteria.MemberId))
        {
            parameters.Add("MemberID", criteria.MemberId);
        }

        parameters.Add("AHAYear", ahaYear);
        parameters.Add("ClaimClass", claimClass);

        if (!string.IsNullOrEmpty(criteria.PayerId))
        {
            parameters.Add("PayerID", criteria.PayerId);
        }

        if (criteria.PageSize > 0)
        {
            parameters.Add("PageSize", criteria.PageSize);
        }

        if (criteria.PageNumber > 0)
        {
            parameters.Add("PageNumber", criteria.PageNumber);
        }

        return parameters;
    }

    private static DataTable BuildNpiTable(IReadOnlyList<string>? npis)
    {
        var table = new DataTable();
        table.Columns.Add("NPI", typeof(string));
        foreach (var npi in npis ?? [])
        {
            table.Rows.Add(npi);
        }

        return table;
    }

    private static async Task<ClaimListResult> ExecuteListAsync(IDbConnection connection, CommandDefinition command, Func<DbDataReader, ClaimListItem> map)
    {
        var items = new List<ClaimListItem>();
        int pageSize = 0, pageNumber = 0, recordTotal = 0;

        await using var reader = (DbDataReader)await connection.ExecuteReaderAsync(command);
        while (await reader.ReadAsync(command.CancellationToken))
        {
            items.Add(map(reader));
            pageSize = GetInt32(reader, "PageSize");
            pageNumber = GetInt32(reader, "PageNumber");
            recordTotal = GetInt32(reader, "RecordTotal");
        }

        return new ClaimListResult { Items = items, PageSize = pageSize, PageNumber = pageNumber, RecordTotal = recordTotal };
    }

    private static ClaimListItem MapPendingItem(DbDataReader reader) => new()
    {
        Id = GetInt64(reader, "ClaimKey"),
        SubmittedDate = GetDateOrNull(reader, "SubmittedDate"),
        BillingNpi = GetString(reader, "billingNPI"),
        RenderingNpi = GetString(reader, "renderingNPI"),
        MemberId = GetString(reader, "memberID"),
        StatusCode = GetString(reader, "ClaimStatus"),
        MemberName = GetString(reader, "memberName"),
        ProviderName = GetString(reader, "ProvName"),
        Source = "E",
        ClaimClass = GetInt32(reader, "ClaimClass"),
        IsEditable = GetBool(reader, "IsEditable"),
        IsPriority = GetBool(reader, "isPriority"),
        AddendumId = GetInt64(reader, "biAddendumID"),
        DayLeft = GetString(reader, "DaysLeft"),
        PayerId = GetString(reader, "PayerID"),
        CanCreate = true,
        PriorityColor = HasColumn(reader, "priorityColor") ? GetString(reader, "priorityColor") : null,
        PriorityLevel = HasColumn(reader, "PriorityLevel") ? GetInt32(reader, "PriorityLevel") : 0,
    };

    private static ClaimListItem MapRejectedItem(DbDataReader reader)
    {
        var checkAmount = GetDecimalOrNull(reader, "checkAmount");
        var checkDate = GetDateOrNull(reader, "checkDate");
        var rejectTypeId = GetInt32OrNull(reader, "RejectTypeID");

        return new ClaimListItem
        {
            Id = GetInt64(reader, "ClaimKey"),
            SubmittedDate = GetDateOrNull(reader, "SubmittedDate"),
            BillingNpi = GetString(reader, "billingNPI"),
            RenderingNpi = GetString(reader, "renderingNPI"),
            MemberId = GetString(reader, "memberID"),
            StatusCode = GetString(reader, "ClaimStatus"),
            MemberName = GetString(reader, "memberName"),
            ProviderName = GetString(reader, "ProvName"),
            Source = "E",
            ClaimClass = GetInt32(reader, "ClaimClass"),
            IsEditable = GetBool(reader, "IsEditable"),
            PaymentStatus = GetString(reader, "paymentStatus"),
            DeniedCode = GetString(reader, "deniedCode"),
            CheckNumber = GetString(reader, "checkNumber"),
            CheckAmount = checkAmount,
            CheckDate = checkDate,
            PaymentBillingNpi = GetString(reader, "PymtBillingNPI"),
            RejectTypeId = rejectTypeId,
            IsPriority = GetBool(reader, "isPriority"),
            AtHome = GetBool(reader, "AtHome"),
            AddendumId = GetInt64(reader, "biAddendumID"),
            HasAddendum = GetBool(reader, "HasAddendum"),
            DayLeft = GetString(reader, "DaysLeft"),
            PayerId = GetString(reader, "PayerID"),
            CanResubmit = true,
            CanViewRejectNotes = true,
            CanEdit = false,
            CanCreate = false,
        };
    }

    // Legacy derives StatusText/StatusTextToolTip/CanEdit/CanViewRejectNotes from StatusCode,
    // RejectTypeID and PaymentStatus once the row is loaded -- preserved verbatim.
    private static ClaimListItem MapSubmittedItem(DbDataReader reader)
    {
        var statusCode = GetString(reader, "ClaimStatus");
        var paymentStatus = GetString(reader, "paymentStatus");
        var checkNumber = GetString(reader, "checkNumber");
        var checkAmount = GetDecimalOrNull(reader, "checkAmount");
        var checkDate = GetDateOrNull(reader, "checkDate");
        var paymentBillingNpi = GetString(reader, "PymtBillingNPI");
        var rejectTypeId = GetInt32OrNull(reader, "RejectTypeID");

        var canEdit = false;
        var canViewRejectNotes = false;
        string? statusText;
        string? statusTextToolTip = null;

        switch (statusCode)
        {
            case "2":
                statusText = "Pending for Quality Review";
                canEdit = true;
                break;

            case "3":
                if (rejectTypeId == 2)
                {
                    statusText = "Administrative Denied";
                    canViewRejectNotes = true;
                }
                else if (rejectTypeId == 3)
                {
                    statusText = "Rejected Expire";
                    canViewRejectNotes = true;
                }
                else
                {
                    statusText = null;
                }

                break;

            case "4":
                switch (paymentStatus?.ToUpperInvariant())
                {
                    case "COMPLETED":
                        statusText = "Completed";
                        break;

                    case "PAID":
                        var formattedCheckDate = checkDate.HasValue ? checkDate.Value.ToShortDateString() : string.Empty;
                        var formattedCheckAmount = checkAmount.HasValue ? checkAmount.Value.ToString("C") : string.Empty;
                        statusText = paymentStatus;
                        statusTextToolTip = $"Check#: {checkNumber?.Trim()}, Amount: {formattedCheckAmount}, Date: {formattedCheckDate}, BillingNPI: {paymentBillingNpi}";
                        break;

                    case "DENIED":
                        var deniedCheckDate = checkDate.HasValue ? checkDate.Value.ToShortDateString() : string.Empty;
                        statusText = "Claim-Denied";
                        statusTextToolTip = $"EOP Date: {deniedCheckDate}";
                        break;

                    default:
                        statusText = "In Process";
                        break;
                }

                break;

            default:
                statusText = string.Empty;
                break;
        }

        var claimClassTagRaw = GetInt32OrNull(reader, "nClaimClassTag");

        return new ClaimListItem
        {
            Id = GetInt64(reader, "ClaimKey"),
            SubmittedDate = GetDateOrNull(reader, "SubmittedDate"),
            BillingNpi = GetString(reader, "billingNPI"),
            RenderingNpi = GetString(reader, "renderingNPI"),
            MemberId = GetString(reader, "memberID"),
            StatusCode = statusCode,
            MemberName = GetString(reader, "memberName"),
            ProviderName = GetString(reader, "ProvName"),
            Source = "E",
            ClaimClass = GetInt32(reader, "ClaimClass"),
            IsEditable = GetBool(reader, "IsEditable"),
            PaymentStatus = paymentStatus,
            DeniedCode = GetString(reader, "deniedCode"),
            CheckNumber = checkNumber,
            CheckAmount = checkAmount,
            CheckDate = checkDate,
            PaymentBillingNpi = paymentBillingNpi,
            RejectTypeId = rejectTypeId,
            IsPriority = GetBool(reader, "isPriority"),
            AtHome = GetBool(reader, "AtHome"),
            AddendumId = GetInt64(reader, "biAddendumID"),
            HasAddendum = GetBool(reader, "HasAddendum"),
            DayLeft = GetString(reader, "DaysLeft"),
            PayerId = GetString(reader, "PayerID"),
            ClaimClassTag = claimClassTagRaw ?? 1,
            CanPrint = true,
            StatusText = statusText,
            StatusTextToolTip = statusTextToolTip,
            CanEdit = canEdit,
            CanViewRejectNotes = canViewRejectNotes,
            ServiceDate = GetDateOrNull(reader, "ServiceDate"),
        };
    }

    private static ClaimListItem MapInProgressItem(DbDataReader reader) => new()
    {
        Id = GetInt64(reader, "ClaimKey"),
        SubmittedDate = GetDateOrNull(reader, "SubmittedDate"),
        BillingNpi = GetString(reader, "billingNPI"),
        RenderingNpi = GetString(reader, "renderingNPI"),
        MemberId = GetString(reader, "memberID"),
        StatusCode = GetString(reader, "ClaimStatus"),
        MemberName = GetString(reader, "memberName"),
        ProviderName = GetString(reader, "ProvName"),
        Source = "E",
        ClaimClass = GetInt32(reader, "ClaimClass"),
        IsEditable = GetBool(reader, "IsEditable"),
        IsPriority = GetBool(reader, "isPriority"),
        AtHome = GetBool(reader, "AtHome"),
        PayerId = GetString(reader, "PayerID"),
        ClaimClassTag = GetInt32(reader, "nClaimClassTag"),
        CanPrint = false,
        ServiceDate = GetDateOrNull(reader, "ServiceDate"),
    };

    // Column types for these list stored procedures aren't confirmed yet (not part of the SP
    // definitions provided so far), so reads go through Convert.ToXxx on the boxed value rather
    // than a strict reader.GetXxx -- this mirrors VB's forgiving CLng/CBool/CDate conversions
    // instead of risking an InvalidCastException on a type guess that's wrong.
    private static object? GetRawValue(DbDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetValue(ordinal);
    }

    private static bool HasColumn(DbDataReader reader, string column)
    {
        for (var i = 0; i < reader.FieldCount; i++)
        {
            if (string.Equals(reader.GetName(i), column, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string? GetString(DbDataReader reader, string column) => GetRawValue(reader, column)?.ToString();

    private static long GetInt64(DbDataReader reader, string column) => GetRawValue(reader, column) is { } v ? Convert.ToInt64(v) : 0L;

    private static int GetInt32(DbDataReader reader, string column) => GetRawValue(reader, column) is { } v ? Convert.ToInt32(v) : 0;

    private static int? GetInt32OrNull(DbDataReader reader, string column) => GetRawValue(reader, column) is { } v ? Convert.ToInt32(v) : null;

    private static bool GetBool(DbDataReader reader, string column) => GetRawValue(reader, column) is { } v && Convert.ToBoolean(v);

    private static decimal? GetDecimalOrNull(DbDataReader reader, string column) => GetRawValue(reader, column) is { } v ? Convert.ToDecimal(v) : null;

    private static DateTime? GetDateOrNull(DbDataReader reader, string column) => GetRawValue(reader, column) is { } v ? Convert.ToDateTime(v) : null;
}
