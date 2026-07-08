namespace Chopper.Services.Providers;

// Mirrors legacy's DataBinding (Value/display-text pair used to populate dropdown-style lists).
public sealed record BillingOption
{
    public string? Value { get; init; }
    public string? DisplayText { get; init; }
}
