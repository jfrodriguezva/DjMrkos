using Dapper;
using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Domain.Promotions;
using DjMrkos.Infrastructure.Persistence;

namespace DjMrkos.Infrastructure.Repositories;

public sealed class PromotionRepository(IResilientDbExecutor db) : IPromotionRepository
{
    public Task<Promotion?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.QueryAsync(async (connection, token) =>
        {
            const string sql = "SELECT * FROM promotions WHERE id = @Id";
            var row = await connection.QuerySingleOrDefaultAsync<PromotionRow>(new CommandDefinition(sql, new { Id = id }, cancellationToken: token));
            return row?.ToEntity();
        }, ct);

    public Task<IReadOnlyList<Promotion>> GetAllAsync(CancellationToken ct) =>
        db.QueryAsync(async (connection, token) =>
        {
            const string sql = "SELECT * FROM promotions ORDER BY created_at_utc DESC";
            var rows = await connection.QueryAsync<PromotionRow>(new CommandDefinition(sql, cancellationToken: token));
            return (IReadOnlyList<Promotion>)rows.Select(r => r.ToEntity()).ToList();
        }, ct);

    public Task<IReadOnlyList<Promotion>> GetAllActiveAsync(CancellationToken ct) =>
        db.QueryAsync(async (connection, token) =>
        {
            const string sql = "SELECT * FROM promotions WHERE is_active = 1";
            var rows = await connection.QueryAsync<PromotionRow>(new CommandDefinition(sql, cancellationToken: token));
            return (IReadOnlyList<Promotion>)rows.Select(r => r.ToEntity()).ToList();
        }, ct);

    public async Task<Promotion> AddAsync(Promotion promotion, CancellationToken ct)
    {
        await db.ExecuteAsync((connection, token) =>
        {
            const string sql = """
                INSERT INTO promotions (id, module_id, category_id, label, discount_percentage, is_active, created_at_utc)
                VALUES (@Id, @ModuleId, @CategoryId, @Label, @DiscountPercentage, @IsActive, @CreatedAtUtc)
                """;
            return connection.ExecuteAsync(new CommandDefinition(sql, PromotionRow.FromEntity(promotion), cancellationToken: token));
        }, ct);

        return promotion;
    }

    public Task UpdateAsync(Promotion promotion, CancellationToken ct) =>
        db.ExecuteAsync((connection, token) =>
        {
            const string sql = """
                UPDATE promotions
                SET label = @Label, discount_percentage = @DiscountPercentage, is_active = @IsActive
                WHERE id = @Id
                """;
            return connection.ExecuteAsync(new CommandDefinition(sql, PromotionRow.FromEntity(promotion), cancellationToken: token));
        }, ct);

    /// <summary>See the remark on <c>ModuleRepository.ModuleRow</c> — init-only properties, no primary constructor.</summary>
    private sealed record PromotionRow
    {
        public Guid Id { get; init; }
        public Guid? ModuleId { get; init; }
        public Guid? CategoryId { get; init; }
        public string Label { get; init; } = string.Empty;
        public decimal DiscountPercentage { get; init; }
        public bool IsActive { get; init; }
        public DateTimeOffset CreatedAtUtc { get; init; }

        public Promotion ToEntity() => Promotion.Rehydrate(Id, ModuleId, CategoryId, Label, DiscountPercentage, IsActive, CreatedAtUtc);

        public static PromotionRow FromEntity(Promotion p) => new()
        {
            Id = p.Id,
            ModuleId = p.ModuleId,
            CategoryId = p.CategoryId,
            Label = p.Label,
            DiscountPercentage = p.DiscountPercentage,
            IsActive = p.IsActive,
            CreatedAtUtc = p.CreatedAtUtc,
        };
    }
}
