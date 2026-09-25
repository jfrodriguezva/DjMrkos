using Dapper;
using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Domain.Leads;
using DjMrkos.Infrastructure.Persistence;

namespace DjMrkos.Infrastructure.Repositories;

public sealed class LeadRepository(IResilientDbExecutor db) : ILeadRepository
{
    public Task<IReadOnlyList<Lead>> GetAllAsync(CancellationToken ct) =>
        db.QueryAsync(async (connection, token) =>
        {
            const string sql = "SELECT * FROM leads ORDER BY created_at_utc DESC";
            var rows = await connection.QueryAsync<LeadRow>(new CommandDefinition(sql, cancellationToken: token));
            return (IReadOnlyList<Lead>)rows.Select(r => r.ToEntity()).ToList();
        }, ct);

    public async Task<Lead> AddAsync(Lead lead, CancellationToken ct)
    {
        await db.ExecuteAsync((connection, token) =>
        {
            const string sql = """
                INSERT INTO leads (id, name, email, phone, event_date, message, status, created_at_utc)
                VALUES (@Id, @Name, @Email, @Phone, @EventDate, @Message, @Status, @CreatedAtUtc)
                """;
            return connection.ExecuteAsync(new CommandDefinition(sql, LeadRow.FromEntity(lead), cancellationToken: token));
        }, ct);

        return lead;
    }

    private sealed record LeadRow(Guid Id, string Name, string Email, string? Phone, DateOnly? EventDate, string Message, int Status, DateTimeOffset CreatedAtUtc)
    {
        public Lead ToEntity() => Lead.Rehydrate(Id, Name, Email, Phone, EventDate, Message, (LeadStatus)Status, CreatedAtUtc);

        public static LeadRow FromEntity(Lead l) => new(l.Id, l.Name, l.Email, l.Phone, l.EventDate, l.Message, (int)l.Status, l.CreatedAtUtc);
    }
}
