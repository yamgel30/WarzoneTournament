namespace Chopper.Services.AhaClaims;

public sealed record CognitiveAssessmentSection
{
    public bool? DayOfTheWeek { get; init; }

    public bool? MonthOfTheYear { get; init; }

    public bool? Year { get; init; }

    public bool? Ball { get; init; }

    public bool? Flag { get; init; }

    public bool? Tree { get; init; }

    public bool? Wnl { get; init; }

    public string? Diagnosis { get; init; }

    public string? PlanGoalsTreatmentInterventionFollowUp { get; init; }
}
