using System.Data;

namespace DjMrkos.Infrastructure.Persistence;

/// <summary>Hands out unopened connections — Dapper opens them lazily, and Polly wraps the attempt.</summary>
public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
