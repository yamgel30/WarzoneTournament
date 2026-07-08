namespace Chopper.Services.Providers;

public sealed record ProviderInfoItem
{
    public string? RenderingNpi { get; init; }
    public string? RenderingName { get; init; }
    public string? DisplayText { get; init; }
    public string? PayerId { get; init; }
    public string? BillingNpi { get; init; }
    public IReadOnlyList<BillingOption> BillingList { get; init; } = [];
}
