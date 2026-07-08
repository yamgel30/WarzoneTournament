namespace Chopper.Services.AhaClaims;

// Read by GetAHA (Page 4) but not currently sent by any ported SaveClaim page -- new DTO, no
// Save-side counterpart to reuse yet.
public sealed record DmeUseSection
{
    public bool? UsingOxygen { get; init; }
    public bool? DueToHypoxiaInAir { get; init; }
    public bool? Cpap { get; init; }
    public bool? AboveKneeProsthesis { get; init; }
    public bool? BelowKneeProsthesis { get; init; }
    public bool? HasSuppliesNeeded { get; init; }
    public bool? Gastrostomy { get; init; }
    public bool? Colostomy { get; init; }
    public bool? Urostomy { get; init; }

    // Legacy checks the DME_Tracheostomy column for null but then assigns the *DME_Urostomy*
    // value to this field (copy-paste bug at the source) -- preserved as-is rather than "fixed",
    // since this is what the legacy app has actually been returning.
    public bool? Tracheostomy { get; init; }

    public bool? UsingWheelchair { get; init; }
    public string? UsingWheelchairReason { get; init; }
    public string? Comments { get; init; }
}
