namespace Chopper.Services.AhaClaims;

public sealed record PhysicalExaminationSection
{
    public decimal? Temperature { get; init; }
    public string? TemperatureType { get; init; }
    public decimal? Pulse { get; init; }
    public decimal? Breathing { get; init; }
    public decimal? BloodPressure1 { get; init; }
    public decimal? BloodPressure2 { get; init; }
    public decimal? Height { get; init; }
    public string? HeightType { get; init; }
    public decimal? Weight { get; init; }
    public string? WeightType { get; init; }
    public decimal? Bmi { get; init; }
    public decimal? HeadCircumference { get; init; }
    public decimal? PercentilWt { get; init; }
    public decimal? PercentilHt { get; init; }
    public decimal? PercentilHead { get; init; }

    public string? HeenOralNotes { get; init; }
    public HeenOralOptions? HeenOralOptions { get; init; }

    public string? ConstitutionalNotes { get; init; }
    public ConstitutionalOptions? ConstitutionalOptions { get; init; }

    public string? IntegumentaryNotes { get; init; }
    public IntegumentaryOptions? IntegumentaryOptions { get; init; }

    public string? RespiratoryNotes { get; init; }
    public RespiratoryOptions? RespiratoryOptions { get; init; }

    public string? GastrointestinalNotes { get; init; }
    public GastrointestinalOptions? GastrointestinalOptions { get; init; }

    public string? GenitourinaryNotes { get; init; }
    public GenitourinaryOptions? GenitourinaryOptions { get; init; }

    public string? NeckNotes { get; init; }
    public NeckOptions? NeckOptions { get; init; }

    public string? ChestNotes { get; init; }
    public ChestOptions? ChestOptions { get; init; }

    public string? CardiovascularNotes { get; init; }
    public CardiovascularOptions? CardiovascularOptions { get; init; }

    public bool? AmputationLegRtBka { get; init; }
    public bool? AmputationLegRtAka { get; init; }
    public bool? AmputationLegRtToe { get; init; }
    public bool? AmputationLegLtBka { get; init; }
    public bool? AmputationLegLtAka { get; init; }
    public bool? AmputationLegLtToe { get; init; }

    public string? AbdomenNotes { get; init; }
    public AbdomenOptions? AbdomenOptions { get; init; }

    public string? GenitaliaGroinButtocksNotes { get; init; }
    public GenitaliaGroinButtocksOptions? GenitaliaGroinButtocksOptions { get; init; }

    public string? MusculoskeletalNotes { get; init; }
    public MusculoskeletalOptions? MusculoskeletalOptions { get; init; }

    public string? SkinNotes { get; init; }
    public SkinOptions? SkinOptions { get; init; }

    public string? PsychiatricNeurologicNotes { get; init; }
    public PsychiatricNeurologicOptions? PsychiatricNeurologicOptions { get; init; }

    public string? HematologicLymphaticImmunologicNotes { get; init; }
    public HematologicLymphaticImmunologicOptions? HematologicLymphaticImmunologicOptions { get; init; }
}
