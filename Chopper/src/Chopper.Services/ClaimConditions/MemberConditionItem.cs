namespace Chopper.Services.ClaimConditions;

public sealed record MemberConditionItem
{
    public long Index { get; init; }
    public string? Condition { get; init; }
    public string? Hcc { get; init; }
    public string? Source { get; init; }
    public string? Detail { get; init; }
}
