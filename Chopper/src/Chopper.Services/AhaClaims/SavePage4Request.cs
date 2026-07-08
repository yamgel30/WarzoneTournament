namespace Chopper.Services.AhaClaims;

public sealed record SavePage4Request
{
    public BmiAssociatedDiagnosesSection? BmiAssociatedDiagnoses { get; init; }

    public RheumatoidArthritisSection? RheumatoidArthritis { get; init; }

    public AssessmentPlanOfTreatmentSection? AssessmentPlanOfTreatment { get; init; }

    public IReadOnlyList<CancerDiagnosisItem>? CancerDiagnoses { get; init; }

    public bool CancerDiagnosisNa { get; init; }

    public ChronicKidneyDiseaseSection? Ckd { get; init; }

    public PressureSoresSection? PressureSores { get; init; }
}
