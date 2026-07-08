namespace Chopper.Services.AhaClaims;

// Fields uspSaveScreeningTest2023 adds on top of ScreeningScheduleSection. The 2023+ form drops
// COVID-19 vaccine tracking and the microalbumin block, and replaces GlaucomaTestDate with nothing
// (Glaucoma result/NAFor/prescribed are still sent, just no date) -- those fields on
// ScreeningScheduleSection are simply not sent for a 2023+ save.
public sealed record ScreeningSchedule2023Extras
{
    public bool? Retinopathy { get; init; }
    public bool? Proliferative { get; init; }
    public bool? ProliferativeEyeRt { get; init; }
    public bool? ProliferativeEyeLt { get; init; }

    public DateTime? UrineAlbuminDate { get; init; }
    public decimal? UrineAlbuminResult { get; init; }
    public string? UrineAlbuminNaFor { get; init; }
    public bool? UrineAlbuminPrescribed { get; init; }

    public DateTime? UrineCreatinineDate { get; init; }
    public decimal? UrineCreatinineResult { get; init; }
    public string? UrineCreatinineNaFor { get; init; }
    public bool? UrineCreatininePrescribed { get; init; }

    public decimal? CreatinineAlbuminRatio { get; init; }

    public string? TdTdapComments { get; init; }
    public bool? TdTdapPrescribed { get; init; }
    public bool? TdTdapPatientRefuses { get; init; }
    public DateTime? TdTdapDoneDate { get; init; }

    public bool? ZosterVaccineOrdered { get; init; }
    public bool? ZosterVaccineRefuse { get; init; }
    public DateTime? ZosterVaccineShotDate1 { get; init; }
    public DateTime? ZosterVaccineShotDate2 { get; init; }

    public bool? RetinopathyNegative { get; init; }
    public int? RetinopathyNegativeEye { get; init; }
    public bool? EyeSeverity { get; init; }
    public int? EyeSeverityLevel { get; init; }
    public int? ProliferativeEye { get; init; }
    public int? RetinopathyEye { get; init; }
    public bool? MacularEdema { get; init; }
    public int? MacularEdemaEye { get; init; }
    public bool? ScreeningRetinopathyNa { get; init; }
}
