namespace Chopper.Services.AhaClaims;

public sealed record GastrointestinalOptions
{
    public bool? GoodDentation { get; init; }
    public bool? PoorDentation { get; init; }
    public bool? HardToPalpation { get; init; }
    public bool? SoftToPalpation { get; init; }
    public bool? Tenderness { get; init; }
    public bool? Visceromegaly { get; init; }
    public bool? Wnl { get; init; }
}
