using System.Data;
using Microsoft.Data.SqlClient;

namespace Chopper.Services.Common;

internal sealed class SqlConnectionFactory(string connectionString) : ISqlConnectionFactory
{
    public IDbConnection CreateConnection() => new SqlConnection(connectionString);
}
