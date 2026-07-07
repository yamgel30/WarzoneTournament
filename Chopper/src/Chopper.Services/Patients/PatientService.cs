using System.Data;
using Chopper.Services.Abstractions;
using Chopper.Services.Common;
using Dapper;

namespace Chopper.Services.Patients;

internal sealed class PatientService(ISqlConnectionFactory connectionFactory) : IPatientService
{
    // Placeholder stored procedure names — swap for the real legacy SPs once the SOAP source lands.
    private const string GetByIdProcedure = "dbo.usp_Patient_GetById";
    private const string SearchProcedure = "dbo.usp_Patient_Search";

    public async Task<PatientDto?> GetByIdAsync(int patientId, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            GetByIdProcedure,
            new { PatientId = patientId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<PatientDto>(command);
    }

    public async Task<IReadOnlyList<PatientDto>> SearchAsync(string? lastName, CancellationToken cancellationToken = default)
    {
        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            SearchProcedure,
            new { LastName = lastName },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<PatientDto>(command);
        return rows.AsList();
    }
}
