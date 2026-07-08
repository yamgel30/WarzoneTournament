namespace Chopper.Services.AhaClaims;

public sealed record ChronicKidneyDiseaseSection
{
    public bool? Na { get; init; }
    public int? Stage { get; init; }
    public bool? DueToDm { get; init; }
    public string? DueToOtherCondition { get; init; }
    public bool? Controlled { get; init; }
    public bool? LowFatDiet { get; init; }
    public bool? Dialysis { get; init; }
    public bool? NoMeetDialysis { get; init; }
    public string? AdditionalTreatment { get; init; }
    public bool? Hyperparathyroidism { get; init; }
    public string? HyperparathyroidismTreatment { get; init; }
    public string? Gfr { get; init; }
    public string? SerumCalcium { get; init; }
    public string? SerumPth { get; init; }
    public bool? Nephropathy { get; init; }
    public string? NephropathyType { get; init; }
    public bool? Nephritis { get; init; }
    public string? NephritisType { get; init; }
    public bool? HasFistula { get; init; }
    public bool? CkdBox { get; init; }
    public bool? StressIncontinence { get; init; }
    public bool? UrgeIncontinence { get; init; }
    public bool? PostMicturitionDribble { get; init; }
    public bool? OveractiveBladder { get; init; }
    public string? BladderTreatmentPlan { get; init; }
    public bool? KidneyTransplant { get; init; }
    public DateTime? GfrDate { get; init; }
    public bool? GfrOrdered { get; init; }
}
