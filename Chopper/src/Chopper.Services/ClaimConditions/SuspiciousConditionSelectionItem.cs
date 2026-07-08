namespace Chopper.Services.ClaimConditions;

public sealed record SuspiciousConditionSelectionItem
{
    public long ClaimId { get; init; }
    public string? DxCode { get; init; }
    public string? Condition { get; init; }
    public string? Hcc { get; init; }
    public string? Source { get; init; }
    public string? Detail { get; init; }
    public int SelectionIndex { get; init; }

    // Only sent to the V2 stored procedure -- the V1 table type doesn't declare this column.
    public int? ReasonForNo { get; init; }
}
