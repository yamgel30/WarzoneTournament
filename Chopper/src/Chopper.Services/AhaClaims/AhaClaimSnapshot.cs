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

    // List-type data, read from result-set tables other than Tables(0)'s single row. Legacy
    // splits Tables(1) into two disjoint row sets by a DataTable.Select filter on which of
    // Controlled/Remission/Active are null -- see AhaClaimReadService for the exact predicate.
    public IReadOnlyList<CancerDiagnosisItem>? CancerDiagnosis { get; init; }
    public IReadOnlyList<OtherConditionItem>? OtherCurrentConditions { get; init; }
    public IReadOnlyList<PressureSoreListItem>? PressureSoresList { get; init; }
    public IReadOnlyList<DiseasesOfTheSkinItem>? DiseasesOfTheSkin { get; init; }
    public IReadOnlyList<ClaimConditions.DxHistorySelectionItem>? DxHistorySelectionList { get; init; }
    public IReadOnlyList<ClaimConditions.SuspiciousConditionSelectionItem>? SuspiciousDxHxSelectionList { get; init; }
    public MalnutritionCriteriaSection? MalnutritionCriteria { get; init; }
    public IReadOnlyList<ScreeningSubstanceUseItem>? ScreeningSubstanceUseList { get; init; }

    // Sourced from Tables(0), not from the ScreeningSubstanceUseList rows themselves.
    public string? ScreeningSubstanceUseListResult { get; init; }

    // Legacy sets this at the top of the aha object (not nested inside either
    // SocialDeterminants2020 or SocialDeterminants2023), defaulting to false rather than null when
    // the column itself is null.
    public bool? SocialDeterminantsNa { get; init; }

    // Exactly one of these two is populated, chosen the same way SaveClaim does: 2023Section for
    // visits from 2023 onward (except a 2023-exactly + ClaimClass 4 carve-out that still uses the
    // 2020 shape) -- see MapSocialDeterminants.
    public SocialDeterminants2020Section? SocialDeterminants2020 { get; init; }
    public SocialDeterminants2023Section? SocialDeterminants2023 { get; init; }
}
