using System.Data;
using Dapper;

namespace Chopper.Services.Common;

// Legacy derives ClaimClass from the AHAYears table via a plain query rather than a stored
// procedure (GetClaimClassByYear in AHAService1.vb) -- shared by every caller that needs the
// same lookup. Takes an already-open connection so callers can reuse the one they already have
// for the rest of their request instead of opening a second one just for this lookup.
public static class ClaimClassLookup
{
    public static async Task<int> GetByYearAsync(IDbConnection connection, int year, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            "SELECT ay.ClaimClass FROM AHAYears AS ay WHERE ay.AHAYear = @Year",
            new { Year = year },
            cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<int>(command);
    }
}
