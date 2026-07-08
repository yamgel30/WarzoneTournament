namespace Chopper.Services.AhaClaims;

public sealed record Phq9
{
    public int? Q1 { get; init; }
    public int? Q2 { get; init; }
    public int? Q3 { get; init; }
    public int? Q4 { get; init; }
    public int? Q5 { get; init; }
    public int? Q6 { get; init; }
    public int? Q7 { get; init; }
    public int? Q8 { get; init; }
    public int? Q9 { get; init; }
    public int? TotalCol1 { get; init; }
    public int? TotalCol2 { get; init; }
    public int? TotalCol3 { get; init; }
    public bool? NotDifficultAtAll { get; init; }
    public bool? SomewhatDifficult { get; init; }
    public bool? VeryDifficult { get; init; }
    public bool? ExtremelyDifficult { get; init; }
    public bool? DepressedPastYear { get; init; }
    public bool? SuicidePastMonth { get; init; }
    public bool? TriedSuicide { get; init; }
}
