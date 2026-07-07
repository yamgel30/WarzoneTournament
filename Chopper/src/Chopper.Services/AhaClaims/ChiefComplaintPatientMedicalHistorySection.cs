namespace Chopper.Services.AhaClaims;

public sealed record ChiefComplaintPatientMedicalHistorySection
{
    public string? HistoryOfPresentIllness { get; init; }

    public bool? RecentHospitalization { get; init; }

    public DateTime? RecentHospitalizationDate { get; init; }

    public string? AllergiesNotes { get; init; }

    public bool? NoAllergies { get; init; }

    public bool? TotalColectomy { get; init; }

    public string? TotalColectomyDate { get; init; }

    public bool? BilateralMastectomy { get; init; }

    public string? BilateralMastectomyDate { get; init; }

    public string? OtherSurgery { get; init; }

    public string? OtherSurgeryDate { get; init; }

    public bool? NoSurgery { get; init; }

    public bool? UnilateralMastectomyLeft { get; init; }

    public string? UnilateralMastectomyLeftDate { get; init; }

    public bool? UnilateralMastectomyRight { get; init; }

    public string? UnilateralMastectomyRightDate { get; init; }

    public string? HistoryPresentIllnessSelectedText { get; init; }

    public bool? ChoseRiskofHiv { get; init; }

    public bool? ChoseOtherStd { get; init; }

    public bool? ChoseQuittingTabacco { get; init; }

    public bool? ChoseDrinkingAlcohol { get; init; }

    public bool? ChoseUseIllicitDrugs { get; init; }

    public bool? PatientConcentTelecomm { get; init; }
}
