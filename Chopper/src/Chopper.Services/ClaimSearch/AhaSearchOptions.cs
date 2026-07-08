namespace Chopper.Services.ClaimSearch;

// Stand-ins for AppShared.AHAVersion / AppShared.AHAClaimClass / AppShared.MaxDayToResubmit --
// app-wide constants from the legacy AppShared class, whose source isn't available. Configure the
// real values under the "AhaSearch" section once known.
public sealed class AhaSearchOptions
{
    public int DefaultYear { get; init; }
    public int DefaultClaimClass { get; init; }
    public int MaxDayToResubmit { get; init; }
}
