namespace Chopper.Services.AhaClaims;

public sealed record IntegumentaryOptions
{
    public bool? Warm { get; init; }
    public bool? Cold { get; init; }
    public bool? AdequatePerfusion { get; init; }
    public bool? InadequatePerfusion { get; init; }
    public bool? AdequateSkinTurgor { get; init; }
    public bool? InadequateSkinTurgor { get; init; }
    public bool? Acne { get; init; }
    public bool? Rash { get; init; }
    public bool? SkinSpots { get; init; }
    public bool? Others { get; init; }
    public bool? None { get; init; }
}
