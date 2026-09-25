using Dapper;
using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Domain.Modules;
using DjMrkos.Infrastructure.Persistence;

namespace DjMrkos.Infrastructure.Repositories;

public sealed class CategoryRepository(IResilientDbExecutor db) : ICategoryRepository
{
    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.QueryAsync(async (connection, token) =>
        {
            const string sql = "SELECT * FROM categories WHERE id = @Id";
            var row = await connection.QuerySingleOrDefaultAsync<CategoryRow>(new CommandDefinition(sql, new { Id = id }, cancellationToken: token));
            return row?.ToEntity();
        }, ct);

    public Task<IReadOnlyList<Category>> GetByModuleIdAsync(Guid moduleId, bool onlyActive, CancellationToken ct) =>
        db.QueryAsync(async (connection, token) =>
        {
            var sql = onlyActive
                ? "SELECT * FROM categories WHERE module_id = @ModuleId AND is_active = TRUE ORDER BY display_order"
                : "SELECT * FROM categories WHERE module_id = @ModuleId ORDER BY display_order";
            var rows = await connection.QueryAsync<CategoryRow>(new CommandDefinition(sql, new { ModuleId = moduleId }, cancellationToken: token));
            return (IReadOnlyList<Category>)rows.Select(r => r.ToEntity()).ToList();
        }, ct);

    public Task<IReadOnlyList<Category>> GetAllActiveAsync(CancellationToken ct) =>
        db.QueryAsync(async (connection, token) =>
        {
            const string sql = "SELECT * FROM categories WHERE is_active = TRUE ORDER BY module_id, display_order";
            var rows = await connection.QueryAsync<CategoryRow>(new CommandDefinition(sql, cancellationToken: token));
            return (IReadOnlyList<Category>)rows.Select(r => r.ToEntity()).ToList();
        }, ct);

    public async Task<Category> AddAsync(Category category, CancellationToken ct)
    {
        await db.ExecuteAsync((connection, token) =>
        {
            const string sql = """
                INSERT INTO categories (id, module_id, name, slug, description, image_url, display_order, is_active, created_at_utc)
                VALUES (@Id, @ModuleId, @Name, @Slug, @Description, @ImageUrl, @DisplayOrder, @IsActive, @CreatedAtUtc)
                """;
            return connection.ExecuteAsync(new CommandDefinition(sql, CategoryRow.FromEntity(category), cancellationToken: token));
        }, ct);

        return category;
    }

    public Task UpdateAsync(Category category, CancellationToken ct) =>
        db.ExecuteAsync((connection, token) =>
        {
            const string sql = """
                UPDATE categories
                SET name = @Name, slug = @Slug, description = @Description, image_url = @ImageUrl,
                    display_order = @DisplayOrder, is_active = @IsActive
                WHERE id = @Id
                """;
            return connection.ExecuteAsync(new CommandDefinition(sql, CategoryRow.FromEntity(category), cancellationToken: token));
        }, ct);

    public Task DeleteAsync(Guid id, CancellationToken ct) =>
        db.ExecuteAsync((connection, token) =>
        {
            const string sql = "DELETE FROM categories WHERE id = @Id";
            return connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id }, cancellationToken: token));
        }, ct);

    private sealed record CategoryRow(
        Guid Id, Guid ModuleId, string Name, string Slug, string? Description,
        string? ImageUrl, int DisplayOrder, bool IsActive, DateTimeOffset CreatedAtUtc)
    {
        public Category ToEntity() => Category.Rehydrate(Id, ModuleId, Name, Slug, Description, ImageUrl, DisplayOrder, IsActive, CreatedAtUtc);

        public static CategoryRow FromEntity(Category c) => new(
            c.Id, c.ModuleId, c.Name, c.Slug, c.Description, c.ImageUrl, c.DisplayOrder, c.IsActive, c.CreatedAtUtc);
    }
}
