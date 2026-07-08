namespace Chopper.Services.AhaClaims;

public sealed record MedicationListSection
{
    public bool? CurrentlyDoesNotUse { get; init; }
    public IReadOnlyList<MedicationItem>? CurrentMedication { get; init; }
    public IReadOnlyList<MedicationItem>? AdherenceMedicationList { get; init; }

    public bool? NotKnowAllergies { get; init; }
    public IReadOnlyList<MedicationItem>? AllergiesMedicationList { get; init; }
}
