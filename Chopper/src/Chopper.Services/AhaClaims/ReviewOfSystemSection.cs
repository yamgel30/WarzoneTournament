namespace Chopper.Services.AhaClaims;

public sealed record ReviewOfSystemSection
{
    public string? Constitutional { get; init; }

    public string? Heentoral { get; init; }

    public string? AllergicImmunologic { get; init; }

    public string? HematologicLymphatic { get; init; }

    public string? Cardiovascular { get; init; }

    public string? Gastrointestinal { get; init; }

    public string? Genitourinary { get; init; }

    public string? Respiratory { get; init; }

    public string? Musculoskeletal { get; init; }

    public string? Neurological { get; init; }

    public string? Endocrine { get; init; }

    public string? Integumentary { get; init; }

    public string? Psychiatric { get; init; }

    public bool? UrinaryIncontinenceLeaking { get; init; }

    public bool? UrinaryIncontinenceBladderExercises { get; init; }

    public bool? UrinaryIncontinenceTreatmentWithMedicine { get; init; }

    public bool? UrinaryIncontinenceSurgicalIntervention { get; init; }

    public string? DescribePositiveRos { get; init; }

    public string? UrinaryIncontinenceOther { get; init; }

    public bool? UrinaryIncontinenceCheckBoxOther { get; init; }

    public int? HearingDifficulty { get; init; }
}
