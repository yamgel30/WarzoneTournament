namespace Chopper.Services.AhaClaims;

// Read by GetAHA (Page 4) but not currently sent by any ported SaveClaim page -- new DTO, no
// Save-side counterpart to reuse yet.
public sealed record OtherCurrentConditionsAdditionalSection
{
    public string? AdditionalRecomendation { get; init; }
}
