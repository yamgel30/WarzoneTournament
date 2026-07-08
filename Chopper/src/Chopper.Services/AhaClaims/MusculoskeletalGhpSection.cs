namespace Chopper.Services.AhaClaims;

// GHP-only, called alongside GastrointestinalDiseasesSection when IsGhp is true.
public sealed record MusculoskeletalGhpSection
{
    public bool? Na { get; init; }
    public bool? Spondylosis { get; init; }
    public bool? CervicalDiscDisorder { get; init; }
    public bool? CervicothoracicRadiculopathy { get; init; }
    public bool? CervicalRegion { get; init; }
    public bool? CervicothoracicRegion { get; init; }
    public string? MusculoskeletalTreatmentPlan { get; init; }
}
