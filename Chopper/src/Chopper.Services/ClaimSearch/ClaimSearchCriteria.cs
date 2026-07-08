namespace Chopper.Services.ClaimSearch;

public sealed record ClaimSearchCriteria
{
    public IReadOnlyList<string>? RenderingNpiList { get; init; }
    public IReadOnlyList<string>? BillingNpiList { get; init; }

    // Legacy sends these straight through as strings (AddWithValue), letting SQL Server parse
    // them, rather than parsing them client-side first -- kept as strings here to match exactly.
    public string? DateFrom { get; init; }
    public string? DateTo { get; init; }

    public string? MemberId { get; init; }

    // 0 means "not specified"; each endpoint decides independently whether/how to default it.
    public int AhaYear { get; init; }

    public string? PayerId { get; init; }
    public bool AtHome { get; init; }

    // Only read by GetInProgress/GetInProgress2021, which -- unlike the other list endpoints --
    // never derive or override this; whatever the caller sends here goes straight to the SP.
    public int ClaimClass { get; init; }

    public string? FormName { get; init; }
    public int PageSize { get; init; }
    public int PageNumber { get; init; }
    public int ClaimClassTag { get; init; }
}
