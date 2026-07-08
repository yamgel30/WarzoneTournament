namespace Chopper.Services.ClaimSearch;

public sealed record ClaimListResult
{
    public IReadOnlyList<ClaimListItem> Items { get; init; } = [];
    public int PageSize { get; init; }
    public int PageNumber { get; init; }
    public int RecordTotal { get; init; }
}
