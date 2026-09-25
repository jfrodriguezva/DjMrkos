using System.Data;
using Microsoft.Extensions.Options;
using Npgsql;

namespace DjMrkos.Infrastructure.Persistence;

public sealed class NpgsqlConnectionFactory(IOptions<DatabaseOptions> options) : IDbConnectionFactory
{
    private readonly string _connectionString = options.Value.ConnectionString;

    public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
}
