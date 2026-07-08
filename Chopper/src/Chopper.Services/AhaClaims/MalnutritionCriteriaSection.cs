namespace Chopper.Services.AhaClaims;

public sealed record MalnutritionCriteriaSection
{
    public bool? InvoluntaryWeightLoss { get; init; }
    public bool? InvoluntaryWeightLoss10To5At6MonthAnd20To10Over6Month { get; init; }
    public bool? InvoluntaryWeightLossLess5At6MonthAndLess10Over6Month { get; init; }
    public bool? InvoluntaryWeightLossMore10At6MonthAndMore20Over6Month { get; init; }
    public bool? LowBmi { get; init; }
    public bool? LowBmiLess18 { get; init; }
    public bool? LowBmiLess20 { get; init; }
    public bool? ReducedMuscle { get; init; }
    public bool? ReducedMuscleSeverly { get; init; }
    public bool? ReducedMuscleMild { get; init; }
    public bool? ReducedFoodIntake { get; init; }
    public bool? DiseaseBurden { get; init; }
    public bool? OtherCriteria { get; init; }
    public string? OtherCriteriaDescription { get; init; }
    public bool? Albumin { get; init; }
    public bool? Less2Albumin { get; init; }
    public bool? Less25Albumin { get; init; }
    public bool? Less35Albumin { get; init; }
}
