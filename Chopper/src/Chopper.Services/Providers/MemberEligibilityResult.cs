namespace Chopper.Services.Providers;

public sealed record MemberEligibilityResult
{
    public bool Found { get; init; }
    public string? BillingNpi { get; init; }
    public string? Cover { get; init; }
    public DateTime? Dob { get; init; }
    public string? Gender { get; init; }
    public string? MemberId { get; init; }
    public string? Name { get; init; }
    public string? PayerId { get; init; }
    public string? ProviderName { get; init; }
    public string? RenderingNpi { get; init; }
    public string? FirstName { get; init; }
    public string? MiddleName { get; init; }
    public string? LastName { get; init; }
}
