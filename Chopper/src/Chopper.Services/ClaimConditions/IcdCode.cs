namespace Chopper.Services.ClaimConditions;

public sealed record IcdCode
{
    public string? Code { get; init; }
    public string? Description { get; init; }
}
