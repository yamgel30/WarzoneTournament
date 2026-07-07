namespace Chopper.Services.AhaClaims;

public sealed record PainScreeningSection
{
    public bool? PatientHaveComplaint { get; init; }

    public string? PainIsLocated { get; init; }

    public int? RatePainExperiencingNow { get; init; }

    public string? ManagePainWith { get; init; }

    public bool? TreatmentHaveBeenEffective { get; init; }

    public bool? TreatmentHaveBeenEffectiveNa { get; init; }

    public bool? Na { get; init; }

    public bool? BathingDressing { get; init; }

    public bool? Mood { get; init; }

    public bool? WalkingAbility { get; init; }

    public bool? Employment { get; init; }

    public bool? HouseWork { get; init; }

    public bool? Sleep { get; init; }

    public bool? RelationshipWithOther { get; init; }

    public bool? EnjoymentOfLife { get; init; }

    public bool? Transportation { get; init; }

    public bool? Toileting { get; init; }

    public bool? FoodPreparation { get; init; }

    public string? PainDueTo { get; init; }

    public string? PlanGoalsTreatmentInterventionFollowUp { get; init; }

    public string? Others { get; init; }

    public bool? ArthritisDueToInfection { get; init; }

    public bool? PainEvaluationOtherCondition { get; init; }

    public string? PainEvaluationOtherConditionText { get; init; }

    public bool? PainEvaluationOtherActivities { get; init; }
}
