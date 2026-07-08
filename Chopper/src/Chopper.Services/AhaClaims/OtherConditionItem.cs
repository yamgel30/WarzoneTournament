namespace Chopper.Services.AhaClaims;

public sealed record OtherConditionItem
{
    public string? DiagnosesCode { get; init; }
    public string? Diagnoses { get; init; }
    public string? Treatment { get; init; }
    public bool? Controlled { get; init; }
}
