namespace Chopper.Services.AhaClaims;

public sealed record PressureSoreListItem
{
    public bool? ByVaricoseVainsInLegs { get; init; }
    public bool? ByArteriosclerosisInExtremities { get; init; }
    public bool? ByDiabetic { get; init; }
    public bool? ByPressure { get; init; }
    public int? ByPressureStage { get; init; }
    public string? AnatomicalSite { get; init; }
    public string? AnatomicalSiteOther { get; init; }
    public bool? ByOtherCondition { get; init; }
    public string? OtherConditionText { get; init; }
    public string? Treatment { get; init; }
}
