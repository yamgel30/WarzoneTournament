namespace Chopper.Services.AhaClaims;

public sealed record CancerDiagnosisItem
{
    public string? Diagnoses { get; init; }
    public string? Treatment { get; init; }
    public bool? Remission { get; init; }
    public bool? Active { get; init; }
    public bool? History { get; init; }
    public bool? Primary { get; init; }
    public bool? Secondary { get; init; }
    public bool? CurrentlyInChemotherapy { get; init; }
    public bool? CurrentlyInRadiotherapy { get; init; }
    public bool? CurrentlyInImmunotherapy { get; init; }
    public bool? CurrentlyRefusesTreatment { get; init; }
}
