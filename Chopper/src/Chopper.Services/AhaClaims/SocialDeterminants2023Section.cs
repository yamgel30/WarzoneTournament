namespace Chopper.Services.AhaClaims;

// Used for visits from 2023 onward (uspSaveClaims_SocialDeterminants_2023). Not GHP-gated, unlike
// Eyes and Neurology -- this is saved for every 2023+ visit regardless of plan type.
public sealed record SocialDeterminants2023Section
{
    public bool? Na { get; init; }
    public bool? IsAutosufficientInRequestForTransport { get; init; }
    public bool? HasSafeRoof { get; init; }
    public bool? HasSufficientFundsForFood { get; init; }
    public bool? FeelSafeInLivingPlace { get; init; }

    // Read by GetAHA (sourced from Tables(0)'s Social_Determinants_Result) but not currently sent
    // by the Page 4 save path.
    public string? Result { get; init; }
}
