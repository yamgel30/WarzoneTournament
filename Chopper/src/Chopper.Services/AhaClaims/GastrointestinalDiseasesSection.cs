namespace Chopper.Services.AhaClaims;

public sealed record GastrointestinalDiseasesSection
{
    public bool? Na { get; init; }
    public bool? NonalcoholicSteatohepatitis { get; init; }
    public bool? MetabolicSyndrome { get; init; }
    public bool? Hyperkalemia { get; init; }
    public bool? Hypokalemia { get; init; }
    public string? GastrointestinalTreatmentPlan { get; init; }
    public bool? LiverTransplant { get; init; }
    public bool? Gerd { get; init; }
    public bool? ChronicHepatitis { get; init; }
    public bool? DiverticularDisease { get; init; }
    public bool? PepticUlcerDisease { get; init; }
}
