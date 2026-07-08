namespace Chopper.Services.AhaClaims;

public sealed record CongenitalDiseasesSection
{
    public bool? Na { get; init; }
    public bool? SpinaBifida { get; init; }
    public string? SpinaBifidaComments { get; init; }
    public bool? Hydrocephalus { get; init; }
    public string? HydrocephalusComments { get; init; }
    public bool? ChiariMalformation { get; init; }
    public string? ChiariMalformationComments { get; init; }
    public bool? Hemophilia { get; init; }
    public string? HemophiliaComments { get; init; }
    public bool? Cranofacial { get; init; }
    public string? CranofacialComments { get; init; }
    public bool? DistrofiaMuscular { get; init; }
    public string? DistrofiaMuscularComments { get; init; }
    public bool? CerebralPalsy { get; init; }
    public string? CerebralPalsyText { get; init; }
    public string? CerebralPalsyComments { get; init; }
}
