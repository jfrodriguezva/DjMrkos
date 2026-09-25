using Dapper;
using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Domain.Testimonials;
using DjMrkos.Infrastructure.Persistence;

namespace DjMrkos.Infrastructure.Repositories;

public sealed class TestimonialRepository(IResilientDbExecutor db) : ITestimonialRepository
{
    public Task<Testimonial?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.QueryAsync(async (connection, token) =>
        {
            const string sql = "SELECT * FROM testimonials WHERE id = @Id";
            var row = await connection.QuerySingleOrDefaultAsync<TestimonialRow>(new CommandDefinition(sql, new { Id = id }, cancellationToken: token));
            return row?.ToEntity();
        }, ct);

    public Task<IReadOnlyList<Testimonial>> GetApprovedAsync(CancellationToken ct) =>
        db.QueryAsync(async (connection, token) =>
        {
            const string sql = "SELECT * FROM testimonials WHERE is_approved = TRUE ORDER BY created_at_utc DESC";
            var rows = await connection.QueryAsync<TestimonialRow>(new CommandDefinition(sql, cancellationToken: token));
            return (IReadOnlyList<Testimonial>)rows.Select(r => r.ToEntity()).ToList();
        }, ct);

    public Task<IReadOnlyList<Testimonial>> GetAllAsync(CancellationToken ct) =>
        db.QueryAsync(async (connection, token) =>
        {
            const string sql = "SELECT * FROM testimonials ORDER BY created_at_utc DESC";
            var rows = await connection.QueryAsync<TestimonialRow>(new CommandDefinition(sql, cancellationToken: token));
            return (IReadOnlyList<Testimonial>)rows.Select(r => r.ToEntity()).ToList();
        }, ct);

    public async Task<Testimonial> AddAsync(Testimonial testimonial, CancellationToken ct)
    {
        await db.ExecuteAsync((connection, token) =>
        {
            const string sql = """
                INSERT INTO testimonials (id, client_name, event_id, rating, comment, is_approved, created_at_utc)
                VALUES (@Id, @ClientName, @EventId, @Rating, @Comment, @IsApproved, @CreatedAtUtc)
                """;
            return connection.ExecuteAsync(new CommandDefinition(sql, TestimonialRow.FromEntity(testimonial), cancellationToken: token));
        }, ct);

        return testimonial;
    }

    public Task UpdateAsync(Testimonial testimonial, CancellationToken ct) =>
        db.ExecuteAsync((connection, token) =>
        {
            const string sql = "UPDATE testimonials SET is_approved = @IsApproved WHERE id = @Id";
            return connection.ExecuteAsync(new CommandDefinition(sql, TestimonialRow.FromEntity(testimonial), cancellationToken: token));
        }, ct);

    /// <summary>See the remark on <c>ModuleRepository.ModuleRow</c> — init-only properties, no primary constructor.</summary>
    private sealed record TestimonialRow
    {
        public Guid Id { get; init; }
        public string ClientName { get; init; } = string.Empty;
        public Guid? EventId { get; init; }
        public int Rating { get; init; }
        public string Comment { get; init; } = string.Empty;
        public bool IsApproved { get; init; }
        public DateTimeOffset CreatedAtUtc { get; init; }

        public Testimonial ToEntity() => Testimonial.Rehydrate(Id, ClientName, EventId, Rating, Comment, IsApproved, CreatedAtUtc);

        public static TestimonialRow FromEntity(Testimonial t) => new()
        {
            Id = t.Id,
            ClientName = t.ClientName,
            EventId = t.EventId,
            Rating = t.Rating,
            Comment = t.Comment,
            IsApproved = t.IsApproved,
            CreatedAtUtc = t.CreatedAtUtc,
        };
    }
}
