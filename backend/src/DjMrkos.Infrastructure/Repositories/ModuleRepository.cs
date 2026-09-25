using Dapper;
using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Domain.Modules;
using DjMrkos.Infrastructure.Persistence;

namespace DjMrkos.Infrastructure.Repositories;

public sealed class ModuleRepository(IResilientDbExecutor db) : IModuleRepository
{
    public Task<Module?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.QueryAsync(async (connection, token) =>
        {
            const string sql = "SELECT * FROM modules WHERE id = @Id";
            var row = await connection.QuerySingleOrDefaultAsync<ModuleRow>(new CommandDefinition(sql, new { Id = id }, cancellationToken: token));
            return row?.ToEntity();
        }, ct);

    public Task<IReadOnlyList<Module>> GetAllAsync(bool onlyActive, CancellationToken ct) =>
        db.QueryAsync(async (connection, token) =>
        {
            var sql = onlyActive
                ? "SELECT * FROM modules WHERE is_active = TRUE ORDER BY display_order"
                : "SELECT * FROM modules ORDER BY display_order";
            var rows = await connection.QueryAsync<ModuleRow>(new CommandDefinition(sql, cancellationToken: token));
            return (IReadOnlyList<Module>)rows.Select(r => r.ToEntity()).ToList();
        }, ct);

    public async Task<Module> AddAsync(Module module, CancellationToken ct)
    {
        await db.ExecuteAsync((connection, token) =>
        {
            const string sql = """
                INSERT INTO modules (id, name, slug, icon, display_order, is_active, created_at_utc)
                VALUES (@Id, @Name, @Slug, @Icon, @DisplayOrder, @IsActive, @CreatedAtUtc)
                """;
            return connection.ExecuteAsync(new CommandDefinition(sql, ModuleRow.FromEntity(module), cancellationToken: token));
        }, ct);

        return module;
    }

    public Task UpdateAsync(Module module, CancellationToken ct) =>
        db.ExecuteAsync((connection, token) =>
        {
            const string sql = """
                UPDATE modules
                SET name = @Name, slug = @Slug, icon = @Icon, display_order = @DisplayOrder, is_active = @IsActive
                WHERE id = @Id
                """;
            return connection.ExecuteAsync(new CommandDefinition(sql, ModuleRow.FromEntity(module), cancellationToken: token));
        }, ct);

    public Task DeleteAsync(Guid id, CancellationToken ct) =>
        db.ExecuteAsync((connection, token) =>
        {
            const string sql = "DELETE FROM modules WHERE id = @Id";
            return connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id }, cancellationToken: token));
        }, ct);

    /// <summary>
    /// Row shape mirrors the `modules` table. Deliberately a record with only init-only
    /// properties (no primary constructor) — Dapper only honors
    /// <c>DefaultTypeMap.MatchNamesWithUnderscores</c> for property-setter mapping; a
    /// positional record forces Dapper into constructor matching, which expects a parameter
    /// named literally <c>display_order</c> and throws.
    /// </summary>
    private sealed record ModuleRow
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Slug { get; init; } = string.Empty;
        public string? Icon { get; init; }
        public int DisplayOrder { get; init; }
        public bool IsActive { get; init; }
        public DateTimeOffset CreatedAtUtc { get; init; }

        public Module ToEntity() => Module.Rehydrate(Id, Name, Slug, Icon, DisplayOrder, IsActive, CreatedAtUtc);

        public static ModuleRow FromEntity(Module m) => new()
        {
            Id = m.Id,
            Name = m.Name,
            Slug = m.Slug,
            Icon = m.Icon,
            DisplayOrder = m.DisplayOrder,
            IsActive = m.IsActive,
            CreatedAtUtc = m.CreatedAtUtc,
        };
    }
}
