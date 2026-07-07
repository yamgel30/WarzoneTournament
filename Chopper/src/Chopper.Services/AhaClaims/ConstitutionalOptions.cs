namespace Chopper.Services.AhaClaims;

public sealed record ConstitutionalOptions
{
    public bool? WellDeveloped { get; init; }
    public bool? PoorDeveloped { get; init; }
    public bool? AdequateNourishment { get; init; }
    public bool? InadequateNourishment { get; init; }
    public bool? InAcuteDistress { get; init; }
    public bool? NoAcuteDistress { get; init; }
    public bool? Caox { get; init; }
    public bool? Others { get; init; }
    public bool? None { get; init; }
}
