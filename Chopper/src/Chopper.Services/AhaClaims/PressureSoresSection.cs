namespace Chopper.Services.AhaClaims;

public sealed record PressureSoresSection
{
    public bool? Na { get; init; }
    public int? HighBackPressureUlcerStage { get; init; }
    public int? LowBackPressureUlcerStage { get; init; }
    public int? HipPressureUlcerStageLeft { get; init; }
    public int? HipPressureUlcerStageRight { get; init; }
    public bool? HipPressureUlcerLeft { get; init; }
    public bool? HipPressureUlcerRight { get; init; }
    public int? HeelPressureUlcerStageLeft { get; init; }
    public int? HeelPressureUlcerStageRight { get; init; }
    public bool? HeelPressureUlcerLeft { get; init; }
    public bool? HeelPressureUlcerRight { get; init; }
    public int? OtherAreasStage { get; init; }
    public string? OtherAreas { get; init; }
    public bool? Healing { get; init; }
    public bool? Healed { get; init; }
    public bool? Worse { get; init; }
    public bool? Hydrocolloid { get; init; }
    public bool? SilverDressing { get; init; }
    public bool? Hydrogel { get; init; }
    public bool? Antibiotic { get; init; }
    public bool? Alginate { get; init; }
    public bool? Enzyme { get; init; }
    public bool? TransparentDressing { get; init; }
    public string? OthersTreatment { get; init; }
    public bool? ByPressure { get; init; }
    public bool? Chronicle { get; init; }
    public string? AnatomicalSite { get; init; }
}
