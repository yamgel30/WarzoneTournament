namespace Chopper.Services.AhaClaims;

public sealed record DiseasesOfTheSkinItem
{
    public bool? Dermatitis { get; init; }
    public string? DermatitisTreatment { get; init; }
    public string? DermatitisTypeLocation { get; init; }
    public bool? Psoriasis { get; init; }
    public string? PsoriasisType { get; init; }
    public bool? PsoriasicArthritis { get; init; }
    public string? PsoriasicArthritisLocation { get; init; }
    public string? PsoriasisTreatment { get; init; }
    public bool? Ulcer { get; init; }
    public string? UlcerLocationAndDepth { get; init; }
    public bool? DueToArteriosclerosisInExtremities { get; init; }
    public bool? DueToPvd { get; init; }
    public string? UlcerTreatment { get; init; }
    public bool? PressureUlcer { get; init; }
    public int? PressureUlcerStage { get; init; }
    public bool? PressureUlcerNoStage { get; init; }
    public string? PressureUlcerLocation { get; init; }
    public string? PressureUlcerTreatment { get; init; }
    public bool? PressureUlcerOtherCause { get; init; }
    public string? PressureUlcerOtherCauseText { get; init; }
    public string? PressureUlcerOtherCauseTreatment { get; init; }
    public string? UlcerDepth { get; init; }
    public string? UlcerLocation { get; init; }
    public bool? UlcerDueToDiabetes { get; init; }
    public bool? UlcerDueToVaricoseVeins { get; init; }
    public bool? UlcerDueToVaricoseVeinsWithInflamation { get; init; }
    public bool? UlcerDueToIdiopathicVenousHypertension { get; init; }
    public bool? UlcerDueToIdiopathicVenousHypertensionWithInflamation { get; init; }
    public bool? UlcerDueToOtherCause { get; init; }
    public string? UlcerDueToOtherCauseText { get; init; }

    // Read by GetAHA but not currently sent by the Page 4 save path.
    public bool? UlcerDueToPvdWithInflamation { get; init; }
}
