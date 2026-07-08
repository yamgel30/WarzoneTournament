using System.Data;
using Chopper.Services.Abstractions;
using Chopper.Services.ClaimSearch;
using Chopper.Services.Common;
using Dapper;

namespace Chopper.Services.Providers;

internal sealed class ProviderService(ISqlConnectionFactory connectionFactory, AhaSearchOptions options) : IProviderService
{
    public async Task<IReadOnlyList<ProviderBillingItem>> GetBillingOfRenderingAsync(string renderingNpi, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var rows = await QueryBillingsOfRenderingAsync(connection, renderingNpi, cancellationToken);
        return rows
            .Select(r => new ProviderBillingItem { BillingNpi = r.BillingNpi, PayerId = r.PayerId, RenderingNpi = renderingNpi })
            .ToList();
    }

    public async Task<IReadOnlyList<ProviderInfoItem>> GetProvidersInformationAsync(IReadOnlyList<string> renderingNpiList, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var items = new List<ProviderInfoItem>();

        foreach (var npi in renderingNpiList)
        {
            var providerName = await QueryProviderNameAsync(connection, npi, cancellationToken);
            if (providerName is null)
            {
                continue;
            }

            var billingRows = await QueryBillingsOfRenderingAsync(connection, npi, cancellationToken);
            items.Add(new ProviderInfoItem
            {
                RenderingNpi = npi,
                RenderingName = providerName,
                DisplayText = $"{npi} - {providerName}",
                PayerId = string.Empty,
                BillingNpi = string.Empty,
                BillingList = ToBillingOptions(billingRows),
            });
        }

        return items;
    }

    // Legacy's authorized-billing-list variant: the first rendering provider's billing list comes
    // from every authorized billing NPI (uspGetBillingsName); every subsequent provider's billing
    // list is instead filtered down to just the billings that provider actually has among the
    // authorized set (uspGetBillingsOfRendering_FromAuthBilling).
    public async Task<IReadOnlyList<ProviderInfoItem>> GetProvidersInformationFromBillingAsync(
        IReadOnlyList<string> billingNpiList, string? pcpNpi, int ahaYear, string? ipaName, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();

        var renderingNpiList = new List<string>();
        if (!string.IsNullOrEmpty(pcpNpi))
        {
            renderingNpiList.Add(pcpNpi);
        }

        var billingTable = BuildNpiTable(billingNpiList);
        var command = new CommandDefinition(
            "uspGetRenderingListFromBilling",
            new
            {
                AHAYear = ahaYear,
                pcpNPI = pcpNpi ?? string.Empty,
                ipaName,
                BillingNPIs = billingTable.AsTableValuedParameter("Providers"),
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var renderingRows = await connection.QueryAsync(command);
        foreach (var row in renderingRows)
        {
            renderingNpiList.Add((string)row.renderingNPI);
        }

        var items = new List<ProviderInfoItem>();
        foreach (var npi in renderingNpiList)
        {
            var providerName = await QueryProviderNameAsync(connection, npi, cancellationToken);
            if (providerName is null)
            {
                continue;
            }

            var billingList = items.Count == 0
                ? await QueryBillingNamesAsync(connection, billingTable, cancellationToken)
                : await QueryBillingsOfRenderingFromAuthBillingAsync(connection, npi, billingTable, cancellationToken);

            items.Add(new ProviderInfoItem
            {
                RenderingNpi = npi,
                RenderingName = providerName,
                DisplayText = $"{npi} - {providerName}",
                PayerId = string.Empty,
                BillingNpi = string.Empty,
                BillingList = billingList,
            });
        }

        return items;
    }

    public async Task<RenderingNpiLookupResult> GetRenderingNpiOfMemberAsync(string memberId, IReadOnlyList<string> billingNpiList, CancellationToken cancellationToken = default)
        => await LookupRenderingNpiAsync("uspGetRenderingNPIOfMember", memberId, billingNpiList, cancellationToken);

    public async Task<RenderingNpiLookupResult> GetRenderingNpiOfMemberPraiAsync(string memberId, IReadOnlyList<string> billingNpiList, CancellationToken cancellationToken = default)
        => await LookupRenderingNpiAsync("uspGetRenderingNPIOfMemberPRAI", memberId, billingNpiList, cancellationToken);

    public async Task<long> GetPendingClaimAsync(string memberId, string renderingNpi, string billingNpi, bool isShort, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();

        // Legacy overrides the ClaimClass argument entirely with the app-wide constant, and
        // derives Status from it (claimClass * -1) rather than the caller-supplied status.
        var claimClass = options.DefaultClaimClass;
        var command = new CommandDefinition(
            "uspClaim_GetPending",
            new
            {
                ClaimClass = claimClass,
                MemberID = memberId,
                RenderingNPI = renderingNpi,
                BillingNPI = billingNpi,
                Status = claimClass * -1,
                isShort,
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var row = (await connection.QueryAsync(command)).FirstOrDefault();
        return row is null ? 0 : (long)row.biClaimID;
    }

    public async Task<long> GetPendingClaim2023Async(string memberId, string renderingNpi, string billingNpi, bool atHome, bool isShort, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();

        var claimClass = options.DefaultClaimClass;
        var command = new CommandDefinition(
            "uspClaim_GetPending",
            new
            {
                ClaimClass = claimClass,
                MemberID = memberId,
                RenderingNPI = renderingNpi,
                BillingNPI = billingNpi,
                Status = claimClass * -1,
                isShort,
                AtHome = atHome,
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var row = (await connection.QueryAsync(command)).FirstOrDefault();
        return row is null ? 0 : (long)row.biClaimID;
    }

    // Precondition (billingNpi required) is enforced by the caller -- legacy returns a specific
    // "required information" error for that case rather than querying at all.
    public async Task<MemberEligibilityResult> VerifyEligibilityAsync(
        string memberId, DateTime dateOfService, string renderingNpi, string billingNpi, string? payerId, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();

        var dummyNpiTable = BuildNpiTable(options.FirstPlusDummyProviderNpis);
        var command = new CommandDefinition(
            "uspAHAMemberEligibility",
            new
            {
                MemberID = memberId,
                BillingNPI = billingNpi,
                RenderingNPI = renderingNpi,
                ServiceDate = dateOfService,
                PayerID = payerId,
                FirstPlusDummyNPI = dummyNpiTable.AsTableValuedParameter("Providers"),
            },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var row = (await connection.QueryAsync(command)).FirstOrDefault();
        if (row is null)
        {
            return new MemberEligibilityResult { Found = false };
        }

        return new MemberEligibilityResult
        {
            Found = true,
            BillingNpi = (string?)row.billingNPI,
            Cover = (string?)row.contractDesc,
            Dob = row.birthDate,
            Gender = (string?)row.genderID,
            MemberId = memberId,
            Name = (string?)row.name,
            PayerId = (string?)row.payerID,
            ProviderName = (string?)row.providerName,
            RenderingNpi = (string?)row.renderingNPI,
            FirstName = (string?)row.firstName,
            MiddleName = (string?)row.middleName,
            LastName = (string?)row.lastName,
        };
    }

    private async Task<RenderingNpiLookupResult> LookupRenderingNpiAsync(string procedureName, string memberId, IReadOnlyList<string> billingNpiList, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();

        var billingTable = BuildNpiTable(billingNpiList);
        var command = new CommandDefinition(
            procedureName,
            new { memberID = memberId, BillingNPIs = billingTable.AsTableValuedParameter("Providers") },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var row = (await connection.QueryAsync(command)).FirstOrDefault();
        string? renderingNpi = row is null ? null : (string?)row.renderingNPI;
        return new RenderingNpiLookupResult { Found = row is not null, RenderingNpi = renderingNpi };
    }

    private static async Task<string?> QueryProviderNameAsync(IDbConnection connection, string renderingNpi, CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(
            "uspGetProviderInformation",
            new { ProviderNPI = renderingNpi },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var row = (await connection.QueryAsync(command)).FirstOrDefault();
        return row is null ? null : (string?)row.ProvName;
    }

    private static async Task<List<(string? BillingNpi, string? PayerId, string? VendorName)>> QueryBillingsOfRenderingAsync(IDbConnection connection, string renderingNpi, CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(
            "uspGetBillingsOfRendering",
            new { renderingNPI = renderingNpi },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync(command);
        return rows.Select(r => ((string?)r.billingNPI, (string?)r.payerID, (string?)r.vendorName)).ToList();
    }

    private static async Task<IReadOnlyList<BillingOption>> QueryBillingsOfRenderingFromAuthBillingAsync(IDbConnection connection, string renderingNpi, DataTable authBillings, CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(
            "uspGetBillingsOfRendering_FromAuthBilling",
            new { RenderingNPI = renderingNpi, BillingNPIs = authBillings.AsTableValuedParameter("Providers") },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync(command);
        return ToBillingOptions(rows.Select(r => ((string?)r.billingNPI, default(string?), (string?)r.vendorName)));
    }

    private static async Task<IReadOnlyList<BillingOption>> QueryBillingNamesAsync(IDbConnection connection, DataTable billingTable, CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(
            "uspGetBillingsName",
            new { BillingNPIs = billingTable.AsTableValuedParameter("Providers") },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync(command);
        return ToBillingOptions(rows.Select(r => ((string?)r.billingNPI, default(string?), (string?)r.vendorName)));
    }

    private static IReadOnlyList<BillingOption> ToBillingOptions(IEnumerable<(string? BillingNpi, string? PayerId, string? VendorName)> rows) => rows
        .GroupBy(r => r.BillingNpi)
        .Select(g => g.First())
        .Select(r => new BillingOption { Value = r.BillingNpi, DisplayText = $"{r.BillingNpi} - {r.VendorName}" })
        .ToList();

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
}
