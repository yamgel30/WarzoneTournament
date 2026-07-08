namespace Chopper.Services.AhaClaims;

public sealed record HeenOralOptions
{
    public bool? Peerl { get; init; }
    public bool? NoTeeth { get; init; }
    public bool? DryMouth { get; init; }
    public bool? DryNose { get; init; }
    public bool? BleedingGums { get; init; }
    public bool? Wnl { get; init; }
    public bool? Strabismus { get; init; }
    public bool? Ptosis { get; init; }
    public bool? RedReflex { get; init; }
    public bool? AbnormalPupillaryReflex { get; init; }
    public bool? BlockedNasolacrimalDucts { get; init; }
    public bool? NasalDischarge { get; init; }
    public bool? ExudatingTonsils { get; init; }

    // 2025 additions
    public bool? Normocephalic { get; init; }
    public bool? ScalpLessionsMasses { get; init; }
    public bool? NeckSupple { get; init; }
    public bool? Adenopathies { get; init; }
    public bool? ClearOropharynxn { get; init; }
    public bool? LessionExudate { get; init; }
    public bool? TympanicMembranesIntact { get; init; }
    public bool? EqualAirConductionAndAcousticReflexes { get; init; }
    public bool? NoNystagmus { get; init; }
    public bool? Eomi { get; init; }
    public bool? Other { get; init; }
    public bool? None { get; init; }
}
