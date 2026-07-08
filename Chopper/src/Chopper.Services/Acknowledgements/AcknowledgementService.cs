using System.Data;
using Chopper.Services.Abstractions;
using Chopper.Services.Common;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Chopper.Services.Acknowledgements;

internal sealed class AcknowledgementService(
    ISqlConnectionFactory connectionFactory,
    ILogger<AcknowledgementService> logger) : IAcknowledgementService
{
    public Task<bool> LogWelcomeLetterAsync(string renderingNpi, short year, CancellationToken cancellationToken = default) =>
        LogAsync("usp_InserWelcomeLetterAckonledgeLog", renderingNpi, year, cancellationToken);

    public Task<bool> HasWelcomeLetterAsync(string renderingNpi, short year, CancellationToken cancellationToken = default) =>
        HasLogAsync("usp_GetAcknowledgementByRenderingNPI", renderingNpi, year, cancellationToken);

    public Task<bool> LogFunctionalQuadriplegiaMessageAsync(string renderingNpi, short year, CancellationToken cancellationToken = default) =>
        LogAsync("usp_FunctionalQuadriplegiaMessageLog_Insert", renderingNpi, year, cancellationToken);

    public Task<bool> HasFunctionalQuadriplegiaMessageAsync(string renderingNpi, short year, CancellationToken cancellationToken = default) =>
        HasLogAsync("usp_FunctionalQuadriplegiaMessageLogGet", renderingNpi, year, cancellationToken);

    public Task<bool> LogInflammatoryPolyarthritisMessageAsync(string renderingNpi, short year, CancellationToken cancellationToken = default) =>
        LogAsync("usp_InflammatoryPolyarthritisMessage_Insert", renderingNpi, year, cancellationToken);

    public Task<bool> HasInflammatoryPolyarthritisMessageAsync(string renderingNpi, short year, CancellationToken cancellationToken = default) =>
        HasLogAsync("usp_InflammatoryPolyarthritisMessage_Get", renderingNpi, year, cancellationToken);

    // The legacy adapter swallows failures here and returns false instead of a SOAP fault -
    // preserved so existing callers keep seeing the same success/failure signal.
    private async Task<bool> LogAsync(string procedureName, string renderingNpi, short year, CancellationToken cancellationToken)
    {
        try
        {
            using var connection = connectionFactory.CreateConnection();
            var command = new CommandDefinition(
                procedureName,
                new { RenderingNPI = renderingNpi, Year = year },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            await connection.ExecuteAsync(command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to log acknowledgement via {Procedure} for {RenderingNpi}/{Year}", procedureName, renderingNpi, year);
            return false;
        }
    }

    private async Task<bool> HasLogAsync(string procedureName, string renderingNpi, short year, CancellationToken cancellationToken)
    {
        try
        {
            using var connection = connectionFactory.CreateConnection();
            var command = new CommandDefinition(
                procedureName,
                new { RenderingNPI = renderingNpi, Year = year },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var key = await connection.ExecuteScalarAsync<long?>(command);
            return key > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check acknowledgement via {Procedure} for {RenderingNpi}/{Year}", procedureName, renderingNpi, year);
            return false;
        }
    }
}
