namespace Chopper.Services.AhaClaims;

public sealed record CardiovascularDiseasesSection
{
    public bool? Na { get; init; }
    public bool? ArterialHypertension { get; init; }
    public bool? PulmonaryHypertension { get; init; }
    public string? PulmonaryHypertensionType { get; init; }
    public bool? HeartFailure { get; init; }
    public bool? Congestive { get; init; }
    public bool? Diastolic { get; init; }
    public bool? Systolic { get; init; }
    public bool? Chronic { get; init; }
    public bool? Pvd { get; init; }
    public bool? AtrilaFibrillation { get; init; }
    public string? AtrilFibrillationType { get; init; }
    public bool? Arteriosclerosis { get; init; }
    public bool? Aorta { get; init; }
    public bool? Crowns { get; init; }
    public bool? RenalArtery { get; init; }
    public bool? ArteriosclerosisExtremities { get; init; }
    public bool? LegLt { get; init; }
    public bool? LegRt { get; init; }
    public bool? ArmLt { get; init; }
    public bool? ArmRt { get; init; }
    public bool? IntermittentClaudication { get; init; }
    public bool? RestPain { get; init; }
    public bool? OtherComplications { get; init; }
    public string? OtherComplicationsText { get; init; }
    public string? HypertensionTreatmentPlan { get; init; }
    public string? PvdTreatmentPlan { get; init; }
    public string? ArteriosclerosisTreatmentPlan { get; init; }
    public bool? AnginaPectoris { get; init; }
    public bool? Sss { get; init; }
    public bool? Svt { get; init; }
    public bool? Pacemaker { get; init; }
    public bool? Cad { get; init; }
    public bool? Cardiomiopatia { get; init; }
    public bool? MyocardialInfarction { get; init; }
    public bool? Cardiomegaly { get; init; }
    public bool? AtrioventricularBlock { get; init; }
    public string? AtrioventricularBlockDegree { get; init; }
    public bool? VaricoseVeinsOfLowerExtremityWithPain { get; init; }
    public bool? ConductionDisorder { get; init; }
    public string? MyocardialInfarctionTreatmentPlan { get; init; }
    public bool? OldMyocardialInfarction { get; init; }
    public bool? Hyperlipidemia { get; init; }
    public string? HyperlipidemiaText { get; init; }
    public bool? HeartTransplant { get; init; }
}
