namespace Chopper.Services.AhaClaims;

// Read by GetAHA (Page 4) but not currently sent by any ported SaveClaim page -- new DTO, no
// Save-side counterpart to reuse yet.
public sealed record DepressionInventorySection
{
    public int? Choose1 { get; init; }
    public int? Choose2 { get; init; }
    public int? Choose3 { get; init; }
    public int? Choose4 { get; init; }
    public int? Choose5 { get; init; }
    public int? Choose6 { get; init; }
    public int? Choose7 { get; init; }
    public int? Choose8a { get; init; }
    public int? Choose8b { get; init; }
    public int? Choose9 { get; init; }
    public int? Choose10a { get; init; }
    public int? Choose10b { get; init; }
    public bool? IsMild { get; init; }
    public bool? IsSevere { get; init; }
    public bool? IsMajor { get; init; }
    public bool? IsModerate { get; init; }
    public string? PlanOfTreatment { get; init; }
}
