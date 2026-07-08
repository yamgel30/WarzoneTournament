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

    // Page 3
    public ScreeningScheduleSection? ScreeningSchedule { get; init; }

    // Fields legacy reads unconditionally into the same row regardless of visit year (unlike
    // Save, which only sends these to uspSaveScreeningTest2023). Populated for every claim, not
    // just 2023+ ones -- GetAHA has no year branch at this point the way SaveClaim does.
    public ScreeningSchedule2023Extras? ScreeningSchedule2023Extras { get; init; }

    public PhysicalExaminationSection? PhysicalExamination { get; init; }

    // Page 4
    public AssessmentPlanOfTreatmentSection? AssessmentPlanOfTreatment { get; init; }
    public CongenitalDiseasesSection? CongenitalDiseases { get; init; }
    public ChronicKidneyDiseaseSection? Ckd { get; init; }
    public PressureSoresSection? PressureSores { get; init; }
    public RheumatoidArthritisSection? RheumatoidArthritis { get; init; }
    public DepressionInventorySection? DepressionInventory { get; init; }
    public DmeUseSection? DmeUse { get; init; }
    public BmiAssociatedDiagnosesSection? BmiAssociatedDiagnoses { get; init; }
    public MyocardialInfarctionSection? MyocardialInfarction { get; init; }
    public OtherCurrentConditionsAdditionalSection? OtherCurrentConditionsAdditional { get; init; }
    public MajorDepressionSection? MajorDepression { get; init; }
    public CardiovascularDiseasesSection? CardiovascularDiseases { get; init; }
    public PulmonaryDiseasesSection? PulmonaryDiseases { get; init; }

    // Legacy also populates an older aha.Gastrointestinal / aha.Musculoskeletal object pair from
    // this same row, using a strict subset of the columns GastrointestinalDiseasesSection /
    // MusculoskeletalGhpSection already cover -- not duplicated here, since it carries no data the
    // newer sections don't already expose.
    public GastrointestinalDiseasesSection? GastrointestinalDiseases { get; init; }
    public MusculoskeletalGhpSection? MusculoskeletalGhp { get; init; }

    public ImLabRefSection? ImLabRef { get; init; }
    public EyesAndNeurologySection? EyesAndNeurology { get; init; }
}
