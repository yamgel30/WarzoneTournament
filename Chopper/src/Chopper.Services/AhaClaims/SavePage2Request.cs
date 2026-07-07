namespace Chopper.Services.AhaClaims;

public sealed record SavePage2Request
{
    public MedicationReviewSection? MedicationReview { get; init; }

    public CognitiveAssessmentSection? CognitiveAssessment { get; init; }

    public PainScreeningSection? PainScreening { get; init; }

    public ActivitiesOfDailyLivingSection? ActivitiesOfDailyLiving { get; init; }
}
