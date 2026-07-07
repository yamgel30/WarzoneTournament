namespace Chopper.Services.AhaClaims;

public sealed record AbdomenOptions
{
    public bool? Masses { get; init; }
    public bool? Tenderness { get; init; }
    public bool? Hernia { get; init; }
    public bool? LiverEnlargement { get; init; }
    public bool? SpleenEnlargement { get; init; }
    public bool? Colostomy { get; init; }
    public bool? Ileostomy { get; init; }
    public bool? Gastrostomy { get; init; }
    public bool? Wnl { get; init; }
    public bool? Cystostomy { get; init; }
    public bool? UmbilicalInfection { get; init; }
    public bool? Distention { get; init; }
    public bool? Constipation { get; init; }
    public bool? Colics { get; init; }
    public bool? Reflux { get; init; }
    public bool? Rebound { get; init; }
    public bool? Guarding { get; init; }
}
