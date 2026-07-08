namespace Chopper.Services.AhaClaims;

public sealed record AssessmentPlanOfTreatmentSection
{
    // When true, legacy forces the whole diabetic-complication detail below to blank/null before
    // saving, regardless of what the caller sent -- see AhaClaimService for the exact fields.
    public bool? No { get; init; }

    public bool? DmType { get; init; }
    public bool? Controlled { get; init; }
    public bool? PoorlyController { get; init; }
    public string? DmComments { get; init; }
    public bool? DiabeticNeuropathy { get; init; }
    public bool? DiabeticPvd { get; init; }
    public bool? DiabeticNephropathy { get; init; }
    public string? OtherDiabeticComplication { get; init; }
    public string? DiabeticNeuropathyComments { get; init; }
    public string? DiabeticNephropathyComments { get; init; }
    public string? DiabeticPvdComments { get; init; }
    public string? OtherDiabeticComplicationComments { get; init; }
    public bool? DiabeticCataracts { get; init; }
    public string? DiabeticCataractsComments { get; init; }
    public bool? DmSecondary { get; init; }
    public bool? Retinopathy { get; init; }
    public string? RetinopathyComments { get; init; }
    public bool? Proliferative { get; init; }
    public string? ProliferativeComments { get; init; }
    public bool? Dermatitis { get; init; }
    public string? DermatitisComments { get; init; }
    public bool? Periodontal { get; init; }
    public string? PeriodontalComments { get; init; }
    public bool? DiabeticArthropathy { get; init; }
    public string? DiabeticArthropathyComment { get; init; }

    // Not cleared by "No".
    public string? DmSecondaryText { get; init; }
    public bool? OutOfControl { get; init; }
    public bool? UncontrolledWithHyperglycemia { get; init; }
    public bool? UncontrolledWithHypoglycemia { get; init; }
    public bool? HyperlipidemiaDueDm { get; init; }
    public string? DmPlanAndTreatmentComments1 { get; init; }
    public string? DmPlanAndTreatmentComments2 { get; init; }
    public string? DmPlanAndTreatmentComments3 { get; init; }
    public bool? GestionalDiabetes { get; init; }
    public string? GestionalDiabetesComment { get; init; }
    public bool? Remission { get; init; }
}
