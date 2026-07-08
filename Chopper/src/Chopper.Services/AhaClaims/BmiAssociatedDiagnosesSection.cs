namespace Chopper.Services.AhaClaims;

public sealed record BmiAssociatedDiagnosesSection
{
    public bool? Na { get; init; }
    public bool? Obesity { get; init; }
    public bool? MorbidObesity { get; init; }
    public bool? Malnutrition { get; init; }
    public string? EvaluationTreatmentPlan { get; init; }
    public string? MalnutritionGradeTypeText { get; init; }
    public bool? DeficiencyBComplex { get; init; }
    public bool? DeficiencyVitaminB12 { get; init; }
    public bool? DeficiencyVitaminB6 { get; init; }
    public bool? DeficiencyOtherVitaminNutrients { get; init; }
    public string? DeficiencyOtherVitaminNutrientsComments { get; init; }
    public string? MalnutritionScreeningAssesment { get; init; }
}
