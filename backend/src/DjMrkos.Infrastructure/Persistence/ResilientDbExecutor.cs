using System.Data;
using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace DjMrkos.Infrastructure.Persistence;

/// <summary>
/// Polly resilience pipeline for PostgreSQL access, composed outer-to-inner as:
/// retry (up to 3 attempts, exponential backoff with jitter) → circuit breaker (opens after
/// a burst of failures so a dead database fails fast instead of queueing timeouts) → a
/// 5s timeout per individual attempt. Only <see cref="NpgsqlException"/>s flagged
/// <c>IsTransient</c> by Npgsql are retried — a unique-constraint violation or bad SQL
/// fails immediately instead of being retried three times for nothing.
/// </summary>
public sealed class ResilientDbExecutor : IResilientDbExecutor
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ResiliencePipeline _pipeline;

    public ResilientDbExecutor(IDbConnectionFactory connectionFactory, ILogger<ResilientDbExecutor> logger)
    {
        _connectionFactory = connectionFactory;

        _pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<NpgsqlException>(ex => ex.IsTransient),
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
                ShouldHandle = new PredicateBuilder().Handle<NpgsqlException>(ex => ex.IsTransient),
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
