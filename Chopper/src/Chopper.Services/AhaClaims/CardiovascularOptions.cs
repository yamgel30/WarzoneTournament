namespace Chopper.Services.AhaClaims;

public sealed record CardiovascularOptions
{
    public bool? AbnormalHeartSound { get; init; }
    public bool? MurmursDecreasedPedalPulses { get; init; }
    public bool? LegEdema { get; init; }
    public bool? Varicosities { get; init; }
    public bool? AbnormalTemperature { get; init; }
    public bool? Wnl { get; init; }
    public bool? DecreasedPedalPulses { get; init; }

    // 2025 additions
    public bool? RegularRateRhytm { get; init; }
    public bool? IrregularRateRhytm { get; init; }
    public bool? Murmurs { get; init; }
    public bool? Gallops { get; init; }
    public bool? Rubs { get; init; }
    public bool? PainUponPrecordialPalpation { get; init; }
    public bool? Other { get; init; }
    public bool? None { get; init; }
}
