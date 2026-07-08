namespace Chopper.Services.AhaClaims;

public sealed record SavePage4Request
{
    // Legacy reads these off the top-level form header/payer lookup to decide which stored
    // procedures to call (Eye and Neurology's year/GHP branch; also needed by sections not yet
    // ported, e.g. the GHP-only Gastrointestinal/Musculoskeletal pair).
    public DateTime DateOfVisit { get; init; }

    public bool IsGhp { get; init; }

    // Also from the top-level form header -- gates OtherConditionAdditional.
    public bool AtHome { get; init; }

    public BmiAssociatedDiagnosesSection? BmiAssociatedDiagnoses { get; init; }

    public RheumatoidArthritisSection? RheumatoidArthritis { get; init; }

    public AssessmentPlanOfTreatmentSection? AssessmentPlanOfTreatment { get; init; }

    public IReadOnlyList<CancerDiagnosisItem>? CancerDiagnoses { get; init; }

    public bool CancerDiagnosisNa { get; init; }

    public ChronicKidneyDiseaseSection? Ckd { get; init; }

    public PressureSoresSection? PressureSores { get; init; }

    public MajorDepressionSection? MajorDepression { get; init; }

    public CongenitalDiseasesSection? CongenitalDiseases { get; init; }

    public IReadOnlyList<PressureSoreListItem>? PressureSoreList { get; init; }

    public CardiovascularDiseasesSection? CardiovascularDiseases { get; init; }

    public EyesAndNeurologySection? EyesAndNeurology { get; init; }

    public ImLabRefSection? ImLabRef { get; init; }

    public string? OtherConditionAdditionalRecommendation { get; init; }

    public PulmonaryDiseasesSection? PulmonaryDiseases { get; init; }

    public GastrointestinalDiseasesSection? GastrointestinalDiseases { get; init; }

    // GHP-only; not gated by IsGhp here -- SavePage4Async checks that itself.
    public MusculoskeletalGhpSection? MusculoskeletalGhp { get; init; }

    public SocialDeterminants2020Section? SocialDeterminants2020 { get; init; }

    public SocialDeterminants2023Section? SocialDeterminants2023 { get; init; }

    public MalnutritionCriteriaSection? MalnutritionCriteria { get; init; }

    public IReadOnlyList<ScreeningSubstanceUseItem>? ScreeningSubstanceUse { get; init; }

    // Caller-computed summary strings for uspSaveScreeningResult -- legacy derives these from
    // fields not otherwise sent to any stored procedure, so they're taken as-is here.
    public string? ScreeningSubstanceUseResult { get; init; }

    public string? SocialDeterminantsResult { get; init; }

    public string? MalnutritionCriteriaResult { get; init; }
}
