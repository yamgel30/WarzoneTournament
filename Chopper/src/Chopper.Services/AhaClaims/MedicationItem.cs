namespace Chopper.Services.AhaClaims;

public sealed record MedicationItem
{
    public string MedicationName { get; init; } = string.Empty;
    public bool IsHistoric { get; init; }
    public bool IsConfirmed { get; init; }
}
