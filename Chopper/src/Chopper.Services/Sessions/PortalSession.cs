namespace Chopper.Services.Sessions;

public sealed record PortalSession(
    string? AccountNo,
    string? LoginId,
    string? Token,
    string SourceIp,
    string ApplicationUser,
    string PortalId);
