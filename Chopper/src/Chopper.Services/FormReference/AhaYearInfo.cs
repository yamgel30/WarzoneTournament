namespace Chopper.Services.FormReference;

public sealed record AhaYearInfo
{
    public bool Found { get; init; }
    public int Year { get; init; }
    public int ClaimClass { get; init; }
    public bool IsReadOnly { get; init; }
    public DateTime? ReadOnlyDate { get; init; }
    public string? ReadOnlyBy { get; init; }
}
