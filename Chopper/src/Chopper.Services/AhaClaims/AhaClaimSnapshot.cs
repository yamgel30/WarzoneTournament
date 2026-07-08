namespace Chopper.Services.AhaClaims;

// Aggregate read-side view of a claim, built up incrementally as GetAHA is ported section by
// section. Reuses the same plain-value section DTOs SaveClaim writes wherever the fields line up.
public sealed record AhaClaimSnapshot
{
    public AhaFormHeader? Header { get; init; }

    // Page 1
    public ChiefComplaintPatientMedicalHistorySection? ChiefComplaintPatientMedicalHistory { get; init; }
    public MedicalFamilySocialHistorySection? MedicalFamilySocialHistory { get; init; }
    public AdvanceDirectiveSection? AdvanceDirective { get; init; }
    public ReviewOfSystemSection? ReviewOfSystem { get; init; }
    public MedicationListSection? MedicationList { get; init; }

    // Page 2
    public MedicationReviewSection? MedicationReview { get; init; }
    public CognitiveAssessmentSection? CognitiveAssessment { get; init; }
    public PainScreeningSection? PainScreening { get; init; }
    public ActivitiesOfDailyLivingSection? ActivitiesOfDailyLiving { get; init; }
}
