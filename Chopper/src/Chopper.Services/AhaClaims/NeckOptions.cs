namespace Chopper.Services.AhaClaims;

public sealed record NeckOptions
{
    public bool? Masses { get; init; }
    public bool? OverallAppearance { get; init; }
    public bool? Symmetry { get; init; }
    public bool? NormalTrachealPosition { get; init; }
    public bool? Tracheostomy { get; init; }
    public bool? Crepitus { get; init; }
    public bool? ThyroidEnlargement { get; init; }
    public bool? ThyroidTenderness { get; init; }
    public bool? ThyroidMass { get; init; }
    public bool? Wnl { get; init; }
    public bool? Rigity { get; init; }
    public bool? MovementLimitation { get; init; }
    public bool? Crackle { get; init; }
}
