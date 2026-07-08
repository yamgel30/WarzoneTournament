namespace Chopper.Services.FormReference;

public sealed record HistoryPresentIllnessOption
{
    public int Id { get; init; }
    public string? TextToShow { get; init; }
    public string? TextToShowEn { get; init; }
}
