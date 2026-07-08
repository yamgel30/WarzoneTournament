namespace Chopper.Services.ClaimSearch;

// Stand-ins for AppShared.AHAVersion / AppShared.AHAClaimClass / AppShared.MaxDayToResubmit and
// AppSettings("FirstPlusDummyProvider") -- app-wide constants from the legacy AppShared/AppSettings
// config, whose real values aren't available. Configure the real values under the "AhaSearch"
// section once known.
public sealed class AhaSearchOptions
{
    public int DefaultYear { get; init; }
    public int DefaultClaimClass { get; init; }
    public int MaxDayToResubmit { get; init; }
    public IReadOnlyList<string> FirstPlusDummyProviderNpis { get; init; } = [];
}
