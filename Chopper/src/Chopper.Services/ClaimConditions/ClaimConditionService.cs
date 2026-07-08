using System.Data;
using System.Text;
using Chopper.Services.Abstractions;
using Chopper.Services.ClaimSearch;
using Chopper.Services.Common;
using Dapper;

namespace Chopper.Services.ClaimConditions;

internal sealed class ClaimConditionService(ISqlConnectionFactory connectionFactory, AhaSearchOptions options) : IClaimConditionService
{
    public async Task<IReadOnlyList<MemberConditionItem>> GetMemberActiveConditionAsync(string memberId, int? year, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "uspGetMemberActiveCondition",
            new { memberID = memberId, year = year ?? options.DefaultYear },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync(command);
        return rows
            .Select(r => new MemberConditionItem { Index = (long)r.pKey, Condition = (string?)r.condition })
            .ToList();
    }

    public async Task<IReadOnlyList<MemberConditionItem>> GetMemberSuspiciousConditionAsync(string memberId, int? year, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "uspGetMemberSuspiciousCondition",
            new { memberID = memberId, year = year ?? options.DefaultYear },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync(command);
        return rows
            .Select(r => new MemberConditionItem
            {
                Index = (long)r.pKey,
                Condition = (string?)r.condition,
                Hcc = (string?)r.HCC,
                Source = (string?)r.Source,
                Detail = (string?)r.Detail,
            })
            .ToList();
    }

    public async Task<IReadOnlyList<ClaimDiagnosisItem>> GetClaimDiagnosticListAsync(long claimId, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "uspClaim_GetAcceptDx",
            new { ClaimID = claimId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync(command);
        return rows
            .Select(r => new ClaimDiagnosisItem
            {
                Code = (string?)r.DxCode,
                Description = (string?)r.DxDescription,
                HccCms = (string?)r.HCC_CMS ?? string.Empty,
                HccRx = (string?)r.HCC_Rx ?? string.Empty,
            })
            .ToList();
    }

    // uspClaim_GetAHARejectNotes returns two result sets: a single header row (general auditor
    // notes) and a list of per-diagnosis rejection notes. When the header isn't exactly one row,
    // legacy substitutes a fixed message about the 72-hour attachment window instead of querying
    // the diagnosis notes at all.
    public async Task<string> GetRejectNotesAsync(long claimId, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "uspClaim_GetAHARejectNotes",
            new { claimKey = claimId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        using var multi = await connection.QueryMultipleAsync(command);
        var headerRows = (await multi.ReadAsync()).ToList();
        var dxRows = (await multi.ReadAsync()).ToList();

        var msg = new StringBuilder();
        if (headerRows.Count == 1)
        {
            msg.Append("General Notes:").Append('\n');
            msg.Append("   ").Append((string?)headerRows[0].sAuditorNotes).Append('\n');

            var dxCount = 0;
            foreach (var row in dxRows)
            {
                dxCount++;
                if (dxCount == 1)
                {
                    msg.Append('\n').Append("Diagnosis Notes:").Append('\n');
                }

                msg.Append("   ").Append((string?)row.sDx).Append(" - ");

                string? longDescription = row.longDescription;
                if (longDescription is not null)
                {
                    msg.Append(longDescription).Append('\n').Append("   ");
                }

                msg.Append((string?)row.sRejectedNotes).Append('\n').Append('\n');
            }
        }
        else
        {
            msg.Append("General Notes:").Append('\n');
            msg.Append("      Claim is returned because the period to attach document has expired (72hrs).");
        }

        return msg.ToString();
    }

    public async Task<bool> SaveAhaDxHxSelectionAsync(long claimId, IReadOnlyList<DxHistorySelectionItem> items, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();

        var table = new DataTable();
        table.Columns.Add("ClaimID", typeof(long));
        table.Columns.Add("DxCode", typeof(string));
        table.Columns.Add("DxDescription", typeof(string));
        table.Columns.Add("ProviderName", typeof(string));
        table.Columns.Add("Source", typeof(string));
        table.Columns.Add("SelectionIndex", typeof(int));
        table.Columns.Add("ReasonForNo", typeof(short));

        foreach (var item in items)
        {
            table.Rows.Add(item.ClaimId, item.DxCode, item.DxDescription, item.ProviderName, item.Source, item.SelectionIndex, item.ReasonForNo);
        }

        var command = new CommandDefinition(
            "uspAHA_SaveDxHistorySelection",
            new { ClaimID = claimId, AHADxHistSelectionTable = table.AsTableValuedParameter("AHADxHistorySelection") },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var affectedRecords = await connection.ExecuteAsync(command);
        return affectedRecords > 0;
    }

    public Task<IReadOnlyList<DxHistorySelectionItem>> GetAhaDxHxSelectionAsync(long claimId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<DxHistorySelectionItem>>([]);

    public async Task<IReadOnlyList<DxHistorySelectionItem>> GetMemberDxHistoryAsync(string memberId, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "uspAHA_GetMemberDxHistory",
            new { MemberID = memberId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync(command);
        return rows
            .Select(r => new DxHistorySelectionItem
            {
                DxCode = (string?)r.DxCode,
                DxDescription = (string?)r.DxDescription,
                ProviderName = (string?)r.ProviderName,
                Source = (string?)r.Source,
                SelectionIndex = -1,
            })
            .ToList();
    }

    public async Task<bool> SaveSuspiciousConditionDxHxSelectionAsync(long claimId, IReadOnlyList<SuspiciousConditionSelectionItem> items, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();

        var table = new DataTable();
        table.Columns.Add("ClaimID", typeof(long));
        table.Columns.Add("DxCode", typeof(string));
        table.Columns.Add("Condition", typeof(string));
        table.Columns.Add("HCC", typeof(string));
        table.Columns.Add("Source", typeof(string));
        table.Columns.Add("Detail", typeof(string));
        table.Columns.Add("SelectionIndex", typeof(int));

        foreach (var item in items)
        {
            table.Rows.Add(item.ClaimId, item.DxCode, item.Condition, item.Hcc, item.Source, item.Detail, item.SelectionIndex);
        }

        var command = new CommandDefinition(
            "uspAHA_SaveClaims_AHASuspiciousConditionDxHxSelection",
            new { ClaimID = claimId, AHASuspiciousCondDxHistSelectionTable = table.AsTableValuedParameter("AHASuspiciousConditionDxHistorySelection") },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var affectedRecords = await connection.ExecuteAsync(command);
        return affectedRecords > 0;
    }

    public async Task<bool> SaveSuspiciousConditionDxHxSelectionV2Async(long claimId, IReadOnlyList<SuspiciousConditionSelectionItem> items, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();

        var table = new DataTable();
        table.Columns.Add("ClaimID", typeof(long));
        table.Columns.Add("DxCode", typeof(string));
        table.Columns.Add("Condition", typeof(string));
        table.Columns.Add("HCC", typeof(string));
        table.Columns.Add("Source", typeof(string));
        table.Columns.Add("Detail", typeof(string));
        table.Columns.Add("SelectionIndex", typeof(int));
        table.Columns.Add("ReasonForNo", typeof(int));

        foreach (var item in items)
        {
            table.Rows.Add(item.ClaimId, item.DxCode, item.Condition, item.Hcc, item.Source, item.Detail, item.SelectionIndex, item.ReasonForNo);
        }

        var command = new CommandDefinition(
            "uspAHA_SaveClaims_AHASuspiciousConditionDxHxSelection_V2",
            new { ClaimID = claimId, AHASuspiciousCondDxHistSelectionTable = table.AsTableValuedParameter("AHASuspiciousConditionDxHistorySelection_v2") },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var affectedRecords = await connection.ExecuteAsync(command);
        return affectedRecords > 0;
    }

    public async Task<IReadOnlyList<IcdCode>> GetIcdLookupAsync(string searchText, DateTime serviceDate, int ahaYear, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "uspM_ICD_Codes2_Search",
            new { TextToSearch = searchText, ServiceDate = serviceDate, Year = ahaYear },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync(command);
        return rows
            .Select(r => new IcdCode { Code = (string?)r.sICDID, Description = (string?)r.sDescription })
            .ToList();
    }

    public async Task<MemberClaimStatusResult> GetMemberClaimStatusAsync(string memberId, int claimClass, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            "uspClaim_GetClaimStatusByMember",
            new { MemberID = memberId, ClaimClass = claimClass },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var row = (await connection.QueryAsync(command)).FirstOrDefault();
        if (row is null)
        {
            return new MemberClaimStatusResult { CurrentStatus = MemberClaimStatus.Pending };
        }

        long claimId = Convert.ToInt64((object)row.biClaimID);
        int status = Convert.ToInt32((object)row.iStatus);

        // Legacy only ever assigns CurrentStatus for status < 2, = 2, or = 3; any other status
        // value leaves it at the enum's default (unassigned) member, whose real identity depends
        // on the unavailable EnumHelper.MemberClaimStatus declaration order. Mapped to Pending
        // here as the closest guess -- worth confirming once that enum's source is available.
        var currentStatus = status switch
        {
            < 2 => MemberClaimStatus.InProgress,
            2 => MemberClaimStatus.Submitted,
            3 => MemberClaimStatus.Rejected,
            _ => MemberClaimStatus.Pending,
        };

        return new MemberClaimStatusResult { ClaimId = claimId, Status = status, CurrentStatus = currentStatus };
    }
}
