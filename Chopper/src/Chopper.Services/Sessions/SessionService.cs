using System.Data;
using Chopper.Services.Abstractions;
using Chopper.Services.Common;
using Dapper;

namespace Chopper.Services.Sessions;

internal sealed class SessionService(ISqlConnectionFactory connectionFactory) : ISessionService
{
    public async Task<long> LogSessionAsync(PortalSession session, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var parameters = new
        {
            sAccountNo = session.AccountNo ?? string.Empty,
            sLoginID = session.LoginId ?? string.Empty,
            sToken = session.Token ?? string.Empty,
            sSourceIP = session.SourceIp,
            sApplicationUser = session.ApplicationUser,
            sPortalID = session.PortalId,
        };

        var command = new CommandDefinition(
            "uspSessions_Add",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<long>(command);
    }
}
