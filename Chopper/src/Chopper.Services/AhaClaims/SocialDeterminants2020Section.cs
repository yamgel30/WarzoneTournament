namespace Chopper.Services.AhaClaims;

// Used for visits before 2023 (uspSaveClaims_SocialDeterminants).
public sealed record SocialDeterminants2020Section
{
    public bool? Na { get; init; }
    public bool? ProblemsLivingAlone { get; init; }
    public bool? Illiteracy { get; init; }
    public bool? Homelessness { get; init; }
    public bool? InadequateHome { get; init; }
    public bool? DiscordWithNll { get; init; }
    public bool? ProblemsResidentialInstitution { get; init; }
    public bool? LackOfFoodAndWater { get; init; }
    public bool? ExtremePoverty { get; init; }
    public bool? WorriedAboutLosingHousing { get; init; }
    public bool? NotAbleToPayRx { get; init; }
    public bool? NotAbleToPayUtilities { get; init; }
    public bool? NotAbleToPayMedicalCare { get; init; }
    public bool? NotAbleToPayPhone { get; init; }
    public bool? NotAbleToPayTransportation { get; init; }
    public bool? NotAbleToPayClothing { get; init; }
    public bool? ProblemsInRelationship { get; init; }
    public bool? AbsenceFamilyMemberMilitary { get; init; }
    public bool? DisappearanceFamilyMember { get; init; }
    public bool? OtherAbsenceFamilyMember { get; init; }
    public bool? DisruptionSeparation { get; init; }
    public bool? DependentAtHome { get; init; }
    public bool? AlcoholismDrugAddictionFamily { get; init; }
    public bool? InnapropriateDiet { get; init; }
    public bool? OtherReducedMobility { get; init; }
    public bool? NeedPersonalCare { get; init; }
    public bool? NeedAtHome { get; init; }
    public bool? NeedContinuousSupervision { get; init; }
    public bool? OtherProblemsProviderDependency { get; init; }
    public bool? UnavailabilityOtherHelpingAgencies { get; init; }
    public bool? NeedAssisstanceDailyActivities { get; init; }
    public bool? BedriddenFewToNoResources { get; init; }
    public bool? PartialyDependsNoResource { get; init; }

    // Read by GetAHA (sourced from Tables(0)'s Social_Determinants_Result) but not currently sent
    // by the Page 4 save path.
    public string? Result { get; init; }
}
