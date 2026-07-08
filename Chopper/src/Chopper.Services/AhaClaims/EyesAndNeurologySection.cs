namespace Chopper.Services.AhaClaims;

// Shared by uspSaveEyeAndNeurology (pre-2023) and uspSaveEyeAndNeurology2023 -- the two stored
// procedures take almost identical parameters. DementiaSeverity is 2023+-only and simply isn't
// sent for a pre-2023 save.
public sealed record EyesAndNeurologySection
{
    public bool? Na { get; init; }
    public bool? Retinopathy { get; init; }
    public bool? Proliferative { get; init; }
    public bool? ProliferativeEyeRt { get; init; }
    public bool? ProliferativeEyeLt { get; init; }
    public bool? MacularEdema { get; init; }
    public bool? MacularEdemaEyeRt { get; init; }
    public bool? MacularEdemaEyeLt { get; init; }
    public string? OtherComplicationRetinopathy { get; init; }
    public bool? Glaucoma { get; init; }
    public bool? GlaucomaEyeRt { get; init; }
    public bool? GlaucomaEyeLt { get; init; }
    public string? GlaucomaType { get; init; }
    public bool? Cataract { get; init; }
    public bool? CataractRt { get; init; }
    public bool? CataractLt { get; init; }
    public string? CataractType { get; init; }
    public bool? Epilepsy { get; init; }
    public string? EpilepsyType { get; init; }
    public bool? Seizures { get; init; }
    public string? SeizuresCause { get; init; }
    public bool? Polyneuropathy { get; init; }
    public string? PolyneuropathyDueTo { get; init; }
    public bool? Neuropathy { get; init; }
    public bool? AutonomicNeuropathy { get; init; }
    public bool? Mononeuritis { get; init; }
    public bool? Neuralgia { get; init; }
    public string? PolyneuropathyOtherSpecification { get; init; }
    public string? RetinopathyTreatmentPlan { get; init; }
    public string? GlaucomaTreatmentPlan { get; init; }
    public string? CataractTreatmentPlan { get; init; }
    public string? EpilepsyTreatmentPlan { get; init; }
    public string? PolyneuropathyTreatmentPlan { get; init; }
    public bool? PolyneuropathyDueToCkb { get; init; }
    public bool? RetinopathyEyeRt { get; init; }
    public bool? RetinopathyEyeLt { get; init; }
    public bool? AlzheimerDisease { get; init; }
    public bool? Dementia { get; init; }
    public int? RetinopathySeverity { get; init; }
    public int? ProliferativeSeverity { get; init; }
    public string? ProliferativeTreatmentPlan { get; init; }
    public string? DementiaAlzheimerTreatmentPlan { get; init; }

    // 2023+ only.
    public int? DementiaSeverity { get; init; }
}
