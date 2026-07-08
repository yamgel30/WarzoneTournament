using Chopper.Services.Sessions;

namespace Chopper.Services.Abstractions;

public interface ISessionService
{
    Task<long> LogSessionAsync(PortalSession session, CancellationToken cancellationToken = default);
}
