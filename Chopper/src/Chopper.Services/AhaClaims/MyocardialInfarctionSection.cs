namespace Chopper.Services.AhaClaims;

public sealed record MyocardialInfarctionSection
{
    public bool? OldMi { get; init; }

    public bool? BetaBlocker { get; init; }

    public string? BetaBlockerType { get; init; }

    public string? OtherTreatmentCircumstances { get; init; }

    public bool? Ami6Months { get; init; }
}
