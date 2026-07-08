namespace Chopper.Services.AhaClaims;

public sealed record SavePage4Request
{
    // Legacy reads these off the top-level form header/payer lookup to decide which stored
    // procedures to call (Eye and Neurology's year/GHP branch; also needed by sections not yet
    // ported, e.g. the GHP-only Gastrointestinal/Musculoskeletal pair).
    public DateTime DateOfVisit { get; init; }

    public bool IsGhp { get; init; }

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
}
