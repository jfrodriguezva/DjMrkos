using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace DjMrkos.Infrastructure.Persistence;

/// <summary>
/// Polly resilience pipeline for SQL Server access, composed outer-to-inner as:
/// retry (up to 3 attempts, exponential backoff with jitter) → circuit breaker (opens after
/// a burst of failures so a dead database fails fast instead of queueing timeouts) → a
/// 5s timeout per individual attempt. Only <see cref="SqlException"/>s whose error number is in
/// <see cref="TransientErrorNumbers"/> are retried — a unique-constraint violation or bad SQL
/// fails immediately instead of being retried three times for nothing.
/// </summary>
public sealed class ResilientDbExecutor : IResilientDbExecutor
{
    /// <summary>
    /// SQL Server error numbers that are known to be transient (connection drops, deadlock
    /// victim, Azure SQL throttling/failover, login timeout). Unlike Npgsql, SqlException has
    /// no built-in `IsTransient` flag, so this list is the mechanism.
    /// </summary>
    private static readonly HashSet<int> TransientErrorNumbers = new()
    {
        -2,     // Client timeout
        4060, 40197, 40501, 40613, 49918, 49919, 49920, // Azure SQL throttling / failover
        4221, 615,
        1205,   // Deadlock victim
        10928, 10929, 10053, 10054, 10060, 40143, 233,
    };

    private static bool IsTransient(SqlException ex) => TransientErrorNumbers.Contains(ex.Number);

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ResiliencePipeline _pipeline;

    public ResilientDbExecutor(IDbConnectionFactory connectionFactory, ILogger<ResilientDbExecutor> logger)
    {
        _connectionFactory = connectionFactory;

        _pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<SqlException>(IsTransient),
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromMilliseconds(200),
                UseJitter = true,
                OnRetry = args =>
                {
                    logger.LogWarning(
                        args.Outcome.Exception,
                        "Reintentando operación de base de datos (intento {AttemptNumber}) tras {Delay}",
                        args.AttemptNumber + 1, args.RetryDelay);
                    return default;
                },
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<SqlException>(IsTransient),
                FailureRatio = 0.5,
                MinimumThroughput = 8,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(15),
                OnOpened = args =>
                {
                    logger.LogError("Circuito de base de datos abierto durante {BreakDuration} tras fallos repetidos.", args.BreakDuration);
                    return default;
                },
                OnClosed = _ =>
                {
                    logger.LogInformation("Circuito de base de datos cerrado; la conexión se recuperó.");
                    return default;
                },
            })
            .AddTimeout(new TimeoutStrategyOptions { Timeout = TimeSpan.FromSeconds(5) })
            .Build();
    }

    public Task<T> QueryAsync<T>(Func<IDbConnection, CancellationToken, Task<T>> operation, CancellationToken ct) =>
        _pipeline.ExecuteAsync(async token =>
        {
            using var connection = _connectionFactory.CreateConnection();
            return await operation(connection, token);
        }, ct).AsTask();

    public Task ExecuteAsync(Func<IDbConnection, CancellationToken, Task> operation, CancellationToken ct) =>
        _pipeline.ExecuteAsync(async token =>
        {
            using var connection = _connectionFactory.CreateConnection();
            await operation(connection, token);
            return true;
        }, ct).AsTask();
}
