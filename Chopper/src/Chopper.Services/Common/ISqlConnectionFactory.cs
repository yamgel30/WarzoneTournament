using System.Data;

namespace Chopper.Services.Common;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}
