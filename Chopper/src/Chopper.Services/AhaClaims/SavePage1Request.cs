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

    // MedicationList / AllergiesMedicationList are not yet supported here: the legacy save uses a
    // SQL Server table-valued parameter and the source doesn't specify the server-side type name.
}
