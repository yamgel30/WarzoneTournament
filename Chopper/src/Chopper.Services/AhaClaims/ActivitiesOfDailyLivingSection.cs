namespace Chopper.Services.AhaClaims;

public sealed record ActivitiesOfDailyLivingSection
{
    public bool? Bathing { get; init; }

    public string? BathingComments { get; init; }

    public bool? DressingAndUndressing { get; init; }

    public string? DressingAndUndressingComments { get; init; }

    public bool? Eating { get; init; }

    public string? EatingComments { get; init; }

    public bool? TransferringBedChair { get; init; }

    public string? TransferringBedChairComments { get; init; }

    public bool? VoluntarilyControl { get; init; }

    public string? VoluntarilyControlComments { get; init; }

    public bool? UsingToilet { get; init; }

    public string? UsingToiletComments { get; init; }

    public bool? Walking { get; init; }

    public string? WalkingComments { get; init; }

    public bool? BedFast { get; init; }

    public bool? HistoryOfFalling { get; init; }

    public string? HistoryOfFallingComments { get; init; }

    public bool? DependenceOnRespirator { get; init; }

    public bool? DependenceOnWheelchair { get; init; }

    public bool? DependenceOnOxygen { get; init; }
}
