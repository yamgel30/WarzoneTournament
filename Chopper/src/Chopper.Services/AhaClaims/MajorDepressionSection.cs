namespace Chopper.Services.AhaClaims;

public sealed record MajorDepressionSection
{
    public bool? Na { get; init; }
    public bool? IsMajorDepression { get; init; }
    public bool? InRemission { get; init; }
    public bool? Recurrent { get; init; }
    public bool? MildSeverity { get; init; }
    public bool? ModerateSeverity { get; init; }
    public bool? SevereSeverity { get; init; }
    public bool? SeverWithoutPsychoticSymptoms { get; init; }
    public string? TreatmentPlan { get; init; }
    public bool? SingleEpisode { get; init; }
    public bool? PsychoticSymptoms { get; init; }
    public bool? BipolarDisorder { get; init; }
    public string? BipolarDisorderTypeAndSeverity { get; init; }
    public string? BipolarDisorderTreatmentPlan { get; init; }
    public string? SchizophreniaType { get; init; }
    public bool? Schizophrenia { get; init; }
    public string? SchizophreniaTreatmentPlan { get; init; }
    public bool? MoodDisorder { get; init; }
    public string? MoodDisorderComments { get; init; }
    public DateTime? Phq9DoneDate { get; init; }
    public string? Phq9ScoreResult { get; init; }
    public bool? Dysthymia { get; init; }
    public string? DysthymiaComments { get; init; }
    public string? UseOfSubtancesTreatmentPlan { get; init; }
    public int? Phq9ReasonNotDoneId { get; init; }
    public string? Phq9ReasonNotDoneOther { get; init; }
    public string? SubstanceAbuseFreeText { get; init; }
    public DateTime? ScreeningSubstanceUseDatePerformed { get; init; }
    public bool? SubstanceAbuseCheckBox { get; init; }
    public bool? GeneralizedAnxietyDisorder { get; init; }
    public bool? OtherAnxiety { get; init; }
    public string? OtherAnxietyText { get; init; }
    public string? GeneralizedAnxietyDisorderComments { get; init; }
    public Phq9? Phq9 { get; init; }
    public bool? Adhd { get; init; }
    public string? AdhdComments { get; init; }
    public bool? Autism { get; init; }
    public string? AutismComments { get; init; }
}
