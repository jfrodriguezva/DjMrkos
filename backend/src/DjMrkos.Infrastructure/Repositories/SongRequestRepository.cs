using Dapper;
using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Domain.SongRequests;
using DjMrkos.Infrastructure.Persistence;

namespace DjMrkos.Infrastructure.Repositories;

public sealed class SongRequestRepository(IResilientDbExecutor db) : ISongRequestRepository
{
    public Task<SongRequest?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.QueryAsync(async (connection, token) =>
        {
            const string sql = "SELECT * FROM song_requests WHERE id = @Id";
            var row = await connection.QuerySingleOrDefaultAsync<SongRequestRow>(new CommandDefinition(sql, new { Id = id }, cancellationToken: token));
            return row?.ToEntity();
        }, ct);

    public Task<IReadOnlyList<SongRequest>> GetByEventIdAsync(Guid eventId, CancellationToken ct) =>
        db.QueryAsync(async (connection, token) =>
        {
            const string sql = "SELECT * FROM song_requests WHERE event_id = @EventId ORDER BY created_at_utc DESC";
            var rows = await connection.QueryAsync<SongRequestRow>(new CommandDefinition(sql, new { EventId = eventId }, cancellationToken: token));
            return (IReadOnlyList<SongRequest>)rows.Select(r => r.ToEntity()).ToList();
        }, ct);

    public Task<int> CountByFingerprintSinceAsync(Guid eventId, string fingerprint, DateTimeOffset sinceUtc, CancellationToken ct) =>
        db.QueryAsync((connection, token) =>
        {
            const string sql = """
                SELECT COUNT(*) FROM song_requests
                WHERE event_id = @EventId AND requester_fingerprint = @Fingerprint AND created_at_utc >= @SinceUtc
                """;
            return connection.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, new { EventId = eventId, Fingerprint = fingerprint, SinceUtc = sinceUtc }, cancellationToken: token));
        }, ct);

    public async Task<SongRequest> AddAsync(SongRequest songRequest, CancellationToken ct)
    {
        await db.ExecuteAsync((connection, token) =>
        {
            const string sql = """
                INSERT INTO song_requests (id, event_id, song_title, artist, requester_name, dedication, requester_fingerprint, status, created_at_utc)
                VALUES (@Id, @EventId, @SongTitle, @Artist, @RequesterName, @Dedication, @RequesterFingerprint, @Status, @CreatedAtUtc)
                """;
            return connection.ExecuteAsync(new CommandDefinition(sql, SongRequestRow.FromEntity(songRequest), cancellationToken: token));
        }, ct);

        return songRequest;
    }

    public Task UpdateAsync(SongRequest songRequest, CancellationToken ct) =>
        db.ExecuteAsync((connection, token) =>
        {
            const string sql = "UPDATE song_requests SET status = @Status WHERE id = @Id";
            return connection.ExecuteAsync(new CommandDefinition(sql, SongRequestRow.FromEntity(songRequest), cancellationToken: token));
        }, ct);

    private sealed record SongRequestRow(
        Guid Id, Guid EventId, string SongTitle, string? Artist, string? RequesterName,
        string? Dedication, string RequesterFingerprint, int Status, DateTimeOffset CreatedAtUtc)
    {
        public SongRequest ToEntity() => SongRequest.Rehydrate(
            Id, EventId, SongTitle, Artist, RequesterName, Dedication, RequesterFingerprint, (SongRequestStatus)Status, CreatedAtUtc);

        public static SongRequestRow FromEntity(SongRequest s) => new(
            s.Id, s.EventId, s.SongTitle, s.Artist, s.RequesterName, s.Dedication, s.RequesterFingerprint, (int)s.Status, s.CreatedAtUtc);
    }
}
