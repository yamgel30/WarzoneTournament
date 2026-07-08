namespace Chopper.Services.AhaClaims;

public sealed record SkinOptions
{
    public bool? Rashes { get; init; }
    public bool? Lesions { get; init; }
    public bool? Ulcers { get; init; }
    public bool? Nodules { get; init; }
    public bool? Induration { get; init; }
    public bool? Tightening { get; init; }
    public bool? Wnl { get; init; }
    public bool? PurpuricLesionsNoted { get; init; }
}
