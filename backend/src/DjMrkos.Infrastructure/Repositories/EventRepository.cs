using Dapper;
using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Domain.Events;
using DjMrkos.Infrastructure.Persistence;

namespace DjMrkos.Infrastructure.Repositories;

public sealed class EventRepository(IResilientDbExecutor db) : IEventRepository
{
    public Task<Event?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.QueryAsync(async (connection, token) =>
        {
            const string sql = "SELECT * FROM events WHERE id = @Id";
            var row = await connection.QuerySingleOrDefaultAsync<EventRow>(new CommandDefinition(sql, new { Id = id }, cancellationToken: token));
            return row?.ToEntity();
        }, ct);

    public Task<Event?> GetByQrTokenAsync(string qrToken, CancellationToken ct) =>
        db.QueryAsync(async (connection, token) =>
        {
            const string sql = "SELECT * FROM events WHERE qr_token = @QrToken";
            var row = await connection.QuerySingleOrDefaultAsync<EventRow>(new CommandDefinition(sql, new { QrToken = qrToken }, cancellationToken: token));
            return row?.ToEntity();
        }, ct);

    public Task<IReadOnlyList<Event>> GetUpcomingAsync(CancellationToken ct) =>
        db.QueryAsync(async (connection, token) =>
        {
            const string sql = "SELECT * FROM events WHERE status <> 3 ORDER BY event_date_utc";
            var rows = await connection.QueryAsync<EventRow>(new CommandDefinition(sql, cancellationToken: token));
            return (IReadOnlyList<Event>)rows.Select(r => r.ToEntity()).ToList();
        }, ct);

    public async Task<Event> AddAsync(Event @event, CancellationToken ct)
    {
        await db.ExecuteAsync((connection, token) =>
        {
            const string sql = """
                INSERT INTO events (id, client_name, location, event_date_utc, status, qr_token, qr_valid_from_utc, qr_valid_until_utc, created_at_utc)
                VALUES (@Id, @ClientName, @Location, @EventDateUtc, @Status, @QrToken, @QrValidFromUtc, @QrValidUntilUtc, @CreatedAtUtc)
                """;
            return connection.ExecuteAsync(new CommandDefinition(sql, EventRow.FromEntity(@event), cancellationToken: token));
        }, ct);

        return @event;
    }

    public Task UpdateAsync(Event @event, CancellationToken ct) =>
        db.ExecuteAsync((connection, token) =>
        {
            const string sql = """
                UPDATE events
                SET client_name = @ClientName, location = @Location, event_date_utc = @EventDateUtc, status = @Status,
                    qr_token = @QrToken, qr_valid_from_utc = @QrValidFromUtc, qr_valid_until_utc = @QrValidUntilUtc
                WHERE id = @Id
                """;
            return connection.ExecuteAsync(new CommandDefinition(sql, EventRow.FromEntity(@event), cancellationToken: token));
        }, ct);

    /// <summary>See the remark on <c>ModuleRepository.ModuleRow</c> — init-only properties, no primary constructor.</summary>
    private sealed record EventRow
    {
        public Guid Id { get; init; }
        public string ClientName { get; init; } = string.Empty;
        public string? Location { get; init; }
        public DateTimeOffset EventDateUtc { get; init; }
        public int Status { get; init; }
        public string QrToken { get; init; } = string.Empty;
        public DateTimeOffset QrValidFromUtc { get; init; }
        public DateTimeOffset QrValidUntilUtc { get; init; }
        public DateTimeOffset CreatedAtUtc { get; init; }

        public Event ToEntity() => Event.Rehydrate(
            Id, ClientName, Location, EventDateUtc, (EventStatus)Status, QrToken, QrValidFromUtc, QrValidUntilUtc, CreatedAtUtc);

        public static EventRow FromEntity(Event e) => new()
        {
            Id = e.Id,
            ClientName = e.ClientName,
            Location = e.Location,
            EventDateUtc = e.EventDateUtc,
            Status = (int)e.Status,
            QrToken = e.QrToken,
            QrValidFromUtc = e.QrValidFromUtc,
            QrValidUntilUtc = e.QrValidUntilUtc,
            CreatedAtUtc = e.CreatedAtUtc,
        };
    }
}
