namespace Chopper.Services.ClaimConditions;

public sealed record DxHistorySelectionItem
{
    public long ClaimId { get; init; }
    public string? DxCode { get; init; }
    public string? DxDescription { get; init; }
    public string? ProviderName { get; init; }
    public string? Source { get; init; }

    // 0 = No, 1 = Yes, 2 = Maybe
    public int SelectionIndex { get; init; }
    public short ReasonForNo { get; init; }
}
