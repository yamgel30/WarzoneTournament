namespace Chopper.Services.AhaClaims;

public sealed record MusculoskeletalOptions
{
    public bool? AbnormalGait { get; init; }
    public bool? ClubbingNails { get; init; }
    public bool? CyanosisDigits { get; init; }
    public bool? UpperExtremitiesAsymmetry { get; init; }
    public bool? LowerExtremitiesAsymmetry { get; init; }
    public bool? Dislocation { get; init; }
    public string? DislocationNotes { get; init; }
    public bool? AbnormalMuscleStrengthTone { get; init; }
    public bool? Flaccid { get; init; }
    public bool? CogWheel { get; init; }
    public bool? Spastic { get; init; }
    public bool? AbnormalMovements { get; init; }
    public bool? Wnl { get; init; }

    // 2025 additions
    public bool? NoDeformitiesOrDeformations { get; init; }
    public bool? NormalGait { get; init; }
    public bool? AdequateRom { get; init; }
    public bool? InadequateRom { get; init; }
}
