namespace Chopper.Services.AhaClaims;

public sealed record PulmonaryDiseasesSection
{
    public bool? Na { get; init; }
    public bool? Asthma { get; init; }
    public string? AsthmaComments { get; init; }
    public string? AsthmaDescription { get; init; }
    public bool? AcuteBronchitis { get; init; }
    public string? AcuteBronchitisComments { get; init; }
    public bool? ChronicBronchitis { get; init; }
    public string? ChronicBronchitisComments { get; init; }
    public bool? Copd { get; init; }
    public string? CopdComments { get; init; }
    public bool? PulmonaryFibrosis { get; init; }
    public string? PulmonaryFibrosisComments { get; init; }
    public bool? AcuteLaryngopharyngitis { get; init; }
    public string? AcuteLaryngopharyngitisComments { get; init; }
    public bool? AcuteNasopharyngitis { get; init; }
    public string? AcuteNasopharyngitisComments { get; init; }
    public bool? UpperRespiratoryTractInfection { get; init; }
    public string? UpperRespiratoryTractInfectionComments { get; init; }
    public bool? OtherConditionCheckbox { get; init; }
    public string? OtherCondition { get; init; }
    public string? OtherConditionTreatment { get; init; }
    public bool? LungTransplant { get; init; }
    public string? LungTransplantTreatmentPlan { get; init; }
}
