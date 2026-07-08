namespace Chopper.Services.Abstractions;

public interface IAcknowledgementService
{
    Task<bool> LogWelcomeLetterAsync(string renderingNpi, short year, CancellationToken cancellationToken = default);

    Task<bool> HasWelcomeLetterAsync(string renderingNpi, short year, CancellationToken cancellationToken = default);

    Task<bool> LogFunctionalQuadriplegiaMessageAsync(string renderingNpi, short year, CancellationToken cancellationToken = default);

    Task<bool> HasFunctionalQuadriplegiaMessageAsync(string renderingNpi, short year, CancellationToken cancellationToken = default);

    Task<bool> LogInflammatoryPolyarthritisMessageAsync(string renderingNpi, short year, CancellationToken cancellationToken = default);

    Task<bool> HasInflammatoryPolyarthritisMessageAsync(string renderingNpi, short year, CancellationToken cancellationToken = default);
}
