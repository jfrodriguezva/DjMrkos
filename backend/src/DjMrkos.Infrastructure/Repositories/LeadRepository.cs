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

    /// <summary>See the remark on <c>ModuleRepository.ModuleRow</c> — init-only properties, no primary constructor.</summary>
    private sealed record LeadRow
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string? Phone { get; init; }
        public DateOnly? EventDate { get; init; }
        public string Message { get; init; } = string.Empty;
        public int Status { get; init; }
        public DateTimeOffset CreatedAtUtc { get; init; }

        public Lead ToEntity() => Lead.Rehydrate(Id, Name, Email, Phone, EventDate, Message, (LeadStatus)Status, CreatedAtUtc);

        public static LeadRow FromEntity(Lead l) => new()
        {
            Id = l.Id,
            Name = l.Name,
            Email = l.Email,
            Phone = l.Phone,
            EventDate = l.EventDate,
            Message = l.Message,
            Status = (int)l.Status,
            CreatedAtUtc = l.CreatedAtUtc,
        };
    }
}
