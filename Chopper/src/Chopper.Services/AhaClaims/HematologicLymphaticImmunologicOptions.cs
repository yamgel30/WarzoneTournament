namespace Chopper.Services.AhaClaims;

public sealed record HematologicLymphaticImmunologicOptions
{
    public bool? LymphNodes { get; init; }
    public string? LymphNodesNotes { get; init; }
    public bool? Wnl { get; init; }
}
