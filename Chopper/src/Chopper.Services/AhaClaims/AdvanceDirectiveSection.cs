namespace Chopper.Services.AhaClaims;

public sealed record AdvanceDirectiveSection
{
    public bool? RefuseToCompleteAdvance { get; init; }

    public bool? AdvanceCarePlanDiscussed { get; init; }

    public DateTime? AdvanceCarePlanExecuteOn { get; init; }

    public bool? AdvanceCarePlanExecutedOnCheck { get; init; }
}
