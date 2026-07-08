namespace Chopper.Services.AhaClaims;

public sealed record RheumatoidArthritisSection
{
    public bool? Na { get; init; }
    public bool? RaNoManifestations { get; init; }
    public bool? RaWithPolyneuropathy { get; init; }
    public bool? RaWithMyopathy { get; init; }
    public string? RaOtherManifestations { get; init; }
    public bool? Dmards { get; init; }
    public string? DmardsSpecify { get; init; }
    public bool? PtRefuses { get; init; }
    public string? OtherTreatmentConditions { get; init; }
    public bool? Arthritis { get; init; }
    public string? ArthritisLocationType { get; init; }
    public bool? Nsaids { get; init; }
    public string? NsaidsOtherTreatment { get; init; }
    public string? AffectedJoints { get; init; }
    public bool? InflammatoryPolyarthritis { get; init; }
    public string? InflammatoryPolyarthritisComments { get; init; }
    public bool? ArthropathySequelaViralInfection { get; init; }
    public string? ArthropathySequelaViralInfectionComments { get; init; }
    public bool? ArtritisPsoriatrica { get; init; }
    public bool? Osteoartritis { get; init; }
    public string? ArtritisPsoriatricaComment { get; init; }
    public string? OsteoartritisComment { get; init; }
    public bool? RaOtherManifestationsCheckBox { get; init; }
    public bool? Osteoporosis { get; init; }
    public bool? Osteopenia { get; init; }
    public string? OsteoTreatmentPlan { get; init; }
}
