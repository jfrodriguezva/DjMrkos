using System.Data;

namespace DjMrkos.Infrastructure.Persistence;

/// <summary>
/// Every repository runs its Dapper calls through this instead of opening a connection
/// directly. It is the single seam where Polly's retry/circuit-breaker/timeout policy is
/// applied, so no repository has to know resilience exists.
/// </summary>
public interface IResilientDbExecutor
{
    Task<T> QueryAsync<T>(Func<IDbConnection, CancellationToken, Task<T>> operation, CancellationToken ct);

    Task ExecuteAsync(Func<IDbConnection, CancellationToken, Task> operation, CancellationToken ct);
}
