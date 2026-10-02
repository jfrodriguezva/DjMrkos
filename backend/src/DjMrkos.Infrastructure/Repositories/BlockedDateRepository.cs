using Dapper;
using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Domain.Availability;
using DjMrkos.Infrastructure.Persistence;

namespace DjMrkos.Infrastructure.Repositories;

public sealed class BlockedDateRepository(IResilientDbExecutor db) : IBlockedDateRepository
{
    public Task<BlockedDate?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.QueryAsync(async (connection, token) =>
        {
            const string sql = "SELECT * FROM blocked_dates WHERE id = @Id";
            var row = await connection.QuerySingleOrDefaultAsync<BlockedDateRow>(new CommandDefinition(sql, new { Id = id }, cancellationToken: token));
            return row?.ToEntity();
        }, ct);

    public Task<IReadOnlyList<BlockedDate>> GetFromAsync(DateOnly from, CancellationToken ct) =>
        db.QueryAsync(async (connection, token) =>
        {
            const string sql = "SELECT * FROM blocked_dates WHERE blocked_date >= @From ORDER BY blocked_date";
            var rows = await connection.QueryAsync<BlockedDateRow>(new CommandDefinition(sql, new { From = from }, cancellationToken: token));
            return (IReadOnlyList<BlockedDate>)rows.Select(r => r.ToEntity()).ToList();
        }, ct);

    public Task<IReadOnlySet<DateOnly>> GetExistingAsync(IReadOnlyCollection<DateOnly> dates, CancellationToken ct) =>
        db.QueryAsync(async (connection, token) =>
        {
            if (dates.Count == 0)
                return (IReadOnlySet<DateOnly>)new HashSet<DateOnly>();

            // A range query instead of IN (...): ranges are contiguous and capped at 90 days anyway.
            const string sql = "SELECT blocked_date FROM blocked_dates WHERE blocked_date BETWEEN @From AND @To";
            var found = await connection.QueryAsync<DateOnly>(new CommandDefinition(
                sql, new { From = dates.Min(), To = dates.Max() }, cancellationToken: token));
            var wanted = dates.ToHashSet();
            return (IReadOnlySet<DateOnly>)found.Where(wanted.Contains).ToHashSet();
        }, ct);

    public Task AddRangeAsync(IReadOnlyCollection<BlockedDate> blockedDates, CancellationToken ct) =>
        db.ExecuteAsync((connection, token) =>
        {
            const string sql = """
                INSERT INTO blocked_dates (id, blocked_date, reason, created_at_utc)
                VALUES (@Id, @BlockedDate, @Reason, @CreatedAtUtc)
                """;
            return connection.ExecuteAsync(new CommandDefinition(sql, blockedDates.Select(BlockedDateRow.FromEntity).ToList(), cancellationToken: token));
        }, ct);

    public Task DeleteAsync(Guid id, CancellationToken ct) =>
        db.ExecuteAsync((connection, token) =>
        {
            const string sql = "DELETE FROM blocked_dates WHERE id = @Id";
            return connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id }, cancellationToken: token));
        }, ct);

    /// <summary>See the remark on <c>ModuleRepository.ModuleRow</c> — init-only properties, no primary constructor.</summary>
    private sealed record BlockedDateRow
    {
        public Guid Id { get; init; }
        public DateOnly BlockedDate { get; init; }
        public string? Reason { get; init; }
        public DateTimeOffset CreatedAtUtc { get; init; }

        public BlockedDate ToEntity() => Domain.Availability.BlockedDate.Rehydrate(Id, BlockedDate, Reason, CreatedAtUtc);

        public static BlockedDateRow FromEntity(BlockedDate b) => new()
        {
            Id = b.Id,
            BlockedDate = b.Date,
            Reason = b.Reason,
            CreatedAtUtc = b.CreatedAtUtc,
        };
    }
}
