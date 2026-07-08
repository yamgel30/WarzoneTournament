namespace Chopper.Services.FormReference;

public sealed record AhaFormHeaderSummary
{
    public string? BillingNpi { get; init; }
    public string? RenderingNpi { get; init; }
    public string? MemberId { get; init; }
    public string? MemberName { get; init; }
    public string? MemberFirstName { get; init; }
    public string? MemberMiddleName { get; init; }
    public string? MemberLastName { get; init; }
    public DateTime? MemberDob { get; init; }
    public string? MemberGender { get; init; }
    public string? PayerId { get; init; }
    public string? RenderingName { get; init; }
    public string? ProviderName { get; init; }
    public string? ProviderPostalCity { get; init; }
    public string? ProviderStreetCity { get; init; }
    public string? IpaName { get; init; }

    // Legacy hardcodes this to 2 (Submitted) for every row this query returns.
    public int Status { get; init; } = 2;
}
