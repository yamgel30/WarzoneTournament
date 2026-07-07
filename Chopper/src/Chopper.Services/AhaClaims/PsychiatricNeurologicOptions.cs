namespace Chopper.Services.AhaClaims;

public sealed record PsychiatricNeurologicOptions
{
    public bool? CranialNervesWithDeficits { get; init; }
    public bool? Babinsky { get; init; }
    public bool? SensationByTouch { get; init; }
    public bool? NoSensationTouchLegs { get; init; }
    public bool? OrientedToTime { get; init; }
    public bool? PlaceAndPerson { get; init; }
    public bool? DepressedMode { get; init; }
    public bool? Anxiety { get; init; }
    public bool? Agitation { get; init; }
    public bool? Wnl { get; init; }
    public bool? Hemiplejia { get; init; }
    public bool? Cuadriplejia { get; init; }
    public bool? Paraplejia { get; init; }

    // 2025 additions
    public bool? AmbulatingWoLimitation { get; init; }
    public bool? NormalMuscleStrengthTone { get; init; }
    public bool? AbnormalMuscleStrengthTone { get; init; }
    public bool? FocalDeficits { get; init; }
}
