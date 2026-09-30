using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace DjMrkos.Infrastructure.Persistence;

public sealed class SqlConnectionFactory(IOptions<DatabaseOptions> options) : IDbConnectionFactory
{
    private readonly string _connectionString = options.Value.ConnectionString;

    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
