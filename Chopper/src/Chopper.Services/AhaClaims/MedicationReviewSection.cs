namespace Chopper.Services.AhaClaims;

public sealed record MedicationReviewSection
{
    public bool? Question1 { get; init; }

    public bool? Question2 { get; init; }

    public bool? Question3 { get; init; }
}
