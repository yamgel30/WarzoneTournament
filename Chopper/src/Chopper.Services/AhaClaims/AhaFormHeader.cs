namespace Chopper.Services.AhaClaims;

// The read-side counterpart of the claim's top-level header. Nothing on the Save side models this
// -- SavePage1Request only ever takes AtHome/AccompaniedBy/TypeOfVisit directly -- since the rest
// of these fields are set once at claim creation, outside SaveClaim's page-by-page flow.
public sealed record AhaFormHeader
{
    public long Id { get; init; }
    public DateTime SubmittedDate { get; init; }
    public string? Language { get; init; }
    public string? BillingNpi { get; init; }
    public DateTime? DateOfVisit { get; init; }
    public string? HealthPlan { get; init; }
    public DateTime? MemberDob { get; init; }
    public string? MemberGender { get; init; }
    public string? MemberId { get; init; }
    public string? MemberName { get; init; }
    public string? PayerId { get; init; }
    public string? ProviderName { get; init; }
    public string? RenderingNpi { get; init; }
    public bool? AtHome { get; init; }
    public int? PlaceOfService { get; init; }
    public int? Status { get; init; }
    public DateTime? ApprovedOrRejectedDate { get; init; }
    public string? MemberFirstName { get; init; }
    public string? MemberMiddleName { get; init; }
    public string? MemberLastName { get; init; }
    public string? RenderingName { get; init; }
    public string? BillingName { get; init; }
    public string? IpaName { get; init; }
    public string? AccompaniedBy { get; init; }
    public int? TypeOfVisit { get; init; }
    public long ConcurrencyId { get; init; }
    public int? ClaimClassTag { get; init; }
    public string? MemberLanguage { get; init; }
    public string? MemberLanguageOther { get; init; }
    public string? Race { get; init; }
    public string? Ethnicity { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? AdditionalHealthPlan { get; init; }
    public string? AdditionalHealthPlanOther { get; init; }
    public string? SexualOrientation { get; init; }
    public string? SexAtBirth { get; init; }
    public string? Pronoun { get; init; }
    public string? GenderIdentity { get; init; }
    public string? SexualOrientationSomethingElse { get; init; }
    public string? PronounOtherPronoun { get; init; }
    public string? OtherRace { get; init; }
    public string? GenderIdentityAdditionalGender { get; init; }
}
