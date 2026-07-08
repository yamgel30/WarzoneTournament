namespace Chopper.Services.ClaimSearch;

public sealed record ClaimListItem
{
    public long? Id { get; init; }
    public DateTime? SubmittedDate { get; init; }
    public DateTime? ServiceDate { get; init; }
    public string? MemberId { get; init; }
    public string? MemberName { get; init; }
    public string? BillingNpi { get; init; }
    public string? RenderingNpi { get; init; }
    public string? PayerId { get; init; }
    public string? StatusCode { get; init; }
    public string? Source { get; init; }
    public string? ProviderName { get; init; }
    public int ClaimClass { get; init; }
    public int ClaimClassTag { get; init; }
    public bool IsEditable { get; init; }
    public bool IsPriority { get; init; }
    public bool AtHome { get; init; }
    public long AddendumId { get; init; }
    public bool HasAddendum { get; init; }
    public string? DayLeft { get; init; }
    public string? PriorityColor { get; init; }
    public int PriorityLevel { get; init; }
    public string? PaymentStatus { get; init; }
    public string? DeniedCode { get; init; }
    public string? CheckNumber { get; init; }
    public decimal? CheckAmount { get; init; }
    public DateTime? CheckDate { get; init; }
    public string? PaymentBillingNpi { get; init; }
    public int? RejectTypeId { get; init; }
    public string? StatusText { get; init; }
    public string? StatusTextToolTip { get; init; }
    public bool CanCreate { get; init; }
    public bool CanEdit { get; init; }
    public bool CanPrint { get; init; }
    public bool CanResubmit { get; init; }
    public bool CanViewRejectNotes { get; init; }
}
