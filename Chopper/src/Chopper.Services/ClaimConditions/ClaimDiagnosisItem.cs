namespace Chopper.Services.ClaimConditions;

public sealed record ClaimDiagnosisItem
{
    public string? Code { get; init; }
    public string? Description { get; init; }
    public string? HccCms { get; init; }
    public string? HccRx { get; init; }
}
