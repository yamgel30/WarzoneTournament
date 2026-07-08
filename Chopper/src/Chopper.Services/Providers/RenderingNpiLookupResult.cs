namespace Chopper.Services.Providers;

public sealed record RenderingNpiLookupResult
{
    public bool Found { get; init; }
    public string? RenderingNpi { get; init; }
}
