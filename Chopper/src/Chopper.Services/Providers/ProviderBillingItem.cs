namespace Chopper.Services.Providers;

public sealed record ProviderBillingItem
{
    public string? BillingNpi { get; init; }
    public string? PayerId { get; init; }
    public string? RenderingNpi { get; init; }
}
