namespace Chopper.Services.AhaClaims;

public sealed record SavePage1Request
{
    // Legacy reads these off the top-level form header, not off ChiefComplaintPatientMedicalHistory itself.
    public bool AtHome { get; init; }

    public string? AccompaniedBy { get; init; }

    public int TypeOfVisit { get; init; }

    public ChiefComplaintPatientMedicalHistorySection? ChiefComplaintPatientMedicalHistory { get; init; }

    public MedicalFamilySocialHistorySection? MedicalFamilySocialHistory { get; init; }

    public AdvanceDirectiveSection? AdvanceDirective { get; init; }

    public ReviewOfSystemSection? ReviewOfSystem { get; init; }

    public MyocardialInfarctionSection? MyocardialInfarction { get; init; }

    public MedicationListSection? MedicationList { get; init; }

    // Legacy computes these off the member's payer and date of birth; the caller supplies them
    // directly here since this service has no notion of member/payer records of its own.
    // AllergiesMedicationList is only saved via its own procedure -- and folded into the main
    // MedicationList save as adherence rows -- when IsGhp is true and MemberAge < 21.
    public bool IsGhp { get; init; }

    public decimal? MemberAge { get; init; }
}
