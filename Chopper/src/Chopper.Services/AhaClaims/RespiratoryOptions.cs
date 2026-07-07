namespace Chopper.Services.AhaClaims;

public sealed record RespiratoryOptions
{
    public bool? ClearToAuscultations { get; init; }
    public bool? Wheezes { get; init; }
    public bool? RonchiOrRales { get; init; }
    public bool? AdequatePercussionSounds { get; init; }
    public bool? InadequatePercussionSounds { get; init; }
    public bool? PainUponPalpitation { get; init; }
    public bool? Others { get; init; }
    public bool? None { get; init; }
}
