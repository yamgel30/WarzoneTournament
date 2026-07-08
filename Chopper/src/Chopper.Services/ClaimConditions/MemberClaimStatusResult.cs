namespace Chopper.Services.ClaimConditions;

public enum MemberClaimStatus
{
    Pending,
    InProgress,
    Submitted,
    Rejected,
}

public sealed record MemberClaimStatusResult
{
    public long ClaimId { get; init; }
    public int Status { get; init; }
    public MemberClaimStatus CurrentStatus { get; init; }
}
