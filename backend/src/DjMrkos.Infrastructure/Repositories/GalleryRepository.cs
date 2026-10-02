using Dapper;
using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Domain.Gallery;
using DjMrkos.Infrastructure.Persistence;

namespace DjMrkos.Infrastructure.Repositories;

public sealed class GalleryRepository(IResilientDbExecutor db) : IGalleryRepository
{
    public Task<IReadOnlyList<GalleryAlbumSummary>> GetAlbumSummariesAsync(bool publishedOnly, CancellationToken ct) =>
        db.QueryAsync(async (connection, token) =>
        {
            // Newest events first; albums without a date go last.
            const string sql = """
                SELECT a.*,
                       (SELECT COUNT(*) FROM gallery_images i WHERE i.album_id = a.id) AS image_count,
                       COALESCE(a.cover_image_id,
                                (SELECT TOP 1 i.id FROM gallery_images i WHERE i.album_id = a.id ORDER BY i.created_at_utc, i.id)) AS effective_cover_id
                FROM gallery_albums a
                WHERE @PublishedOnly = 0 OR a.is_published = 1
                ORDER BY CASE WHEN a.event_date IS NULL THEN 1 ELSE 0 END, a.event_date DESC, a.created_at_utc DESC
                """;
            var rows = await connection.QueryAsync<AlbumSummaryRow>(new CommandDefinition(sql, new { PublishedOnly = publishedOnly }, cancellationToken: token));
            return (IReadOnlyList<GalleryAlbumSummary>)rows
                .Select(r => new GalleryAlbumSummary(r.ToEntity(), r.ImageCount, r.EffectiveCoverId))
                .ToList();
        }, ct);

    public Task<GalleryAlbum?> GetAlbumByIdAsync(Guid id, CancellationToken ct) =>
        db.QueryAsync(async (connection, token) =>
        {
            const string sql = "SELECT * FROM gallery_albums WHERE id = @Id";
            var row = await connection.QuerySingleOrDefaultAsync<AlbumRow>(new CommandDefinition(sql, new { Id = id }, cancellationToken: token));
            return row?.ToEntity();
        }, ct);

    public Task<GalleryAlbum?> GetAlbumBySlugAsync(string slug, CancellationToken ct) =>
        db.QueryAsync(async (connection, token) =>
        {
            const string sql = "SELECT * FROM gallery_albums WHERE slug = @Slug";
            var row = await connection.QuerySingleOrDefaultAsync<AlbumRow>(new CommandDefinition(sql, new { Slug = slug }, cancellationToken: token));
            return row?.ToEntity();
        }, ct);

    public Task<bool> SlugExistsAsync(string slug, CancellationToken ct) =>
        db.QueryAsync((connection, token) =>
        {
            const string sql = "SELECT CASE WHEN EXISTS (SELECT 1 FROM gallery_albums WHERE slug = @Slug) THEN 1 ELSE 0 END";
            return connection.ExecuteScalarAsync<bool>(new CommandDefinition(sql, new { Slug = slug }, cancellationToken: token));
        }, ct);

    public Task AddAlbumAsync(GalleryAlbum album, CancellationToken ct) =>
        db.ExecuteAsync((connection, token) =>
        {
            const string sql = """
                INSERT INTO gallery_albums (id, title, slug, event_date, description, cover_image_id, is_published, created_at_utc)
                VALUES (@Id, @Title, @Slug, @EventDate, @Description, @CoverImageId, @IsPublished, @CreatedAtUtc)
                """;
            return connection.ExecuteAsync(new CommandDefinition(sql, AlbumRow.FromEntity(album), cancellationToken: token));
        }, ct);

    public Task UpdateAlbumAsync(GalleryAlbum album, CancellationToken ct) =>
        db.ExecuteAsync((connection, token) =>
        {
            const string sql = """
                UPDATE gallery_albums
                SET title = @Title, event_date = @EventDate, description = @Description,
                    cover_image_id = @CoverImageId, is_published = @IsPublished
                WHERE id = @Id
                """;
            return connection.ExecuteAsync(new CommandDefinition(sql, AlbumRow.FromEntity(album), cancellationToken: token));
        }, ct);

    public Task DeleteAlbumAsync(Guid id, CancellationToken ct) =>
        db.ExecuteAsync((connection, token) =>
        {
            const string sql = "DELETE FROM gallery_albums WHERE id = @Id";
            return connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id }, cancellationToken: token));
        }, ct);

    public Task<IReadOnlyList<GalleryImage>> GetImagesAsync(Guid albumId, CancellationToken ct) =>
        db.QueryAsync(async (connection, token) =>
        {
            const string sql = "SELECT * FROM gallery_images WHERE album_id = @AlbumId ORDER BY created_at_utc, id";
            var rows = await connection.QueryAsync<ImageRow>(new CommandDefinition(sql, new { AlbumId = albumId }, cancellationToken: token));
            return (IReadOnlyList<GalleryImage>)rows.Select(r => r.ToEntity()).ToList();
        }, ct);

    public Task<GalleryImage?> GetImageByIdAsync(Guid id, CancellationToken ct) =>
        db.QueryAsync(async (connection, token) =>
        {
            const string sql = "SELECT * FROM gallery_images WHERE id = @Id";
            var row = await connection.QuerySingleOrDefaultAsync<ImageRow>(new CommandDefinition(sql, new { Id = id }, cancellationToken: token));
            return row?.ToEntity();
        }, ct);

    public Task AddImageAsync(GalleryImage image, CancellationToken ct) =>
        db.ExecuteAsync((connection, token) =>
        {
            const string sql = """
                INSERT INTO gallery_images (id, album_id, caption, width, height, size_bytes, created_at_utc)
                VALUES (@Id, @AlbumId, @Caption, @Width, @Height, @SizeBytes, @CreatedAtUtc)
                """;
            return connection.ExecuteAsync(new CommandDefinition(sql, ImageRow.FromEntity(image), cancellationToken: token));
        }, ct);

    public Task DeleteImageAsync(Guid id, CancellationToken ct) =>
        db.ExecuteAsync((connection, token) =>
        {
            // cover_image_id has no FK (see 0006_GalleryAndBlockedDates.sql), so clear it here.
            const string sql = """
                SET XACT_ABORT ON;
                BEGIN TRANSACTION;
                UPDATE gallery_albums SET cover_image_id = NULL WHERE cover_image_id = @Id;
                DELETE FROM gallery_images WHERE id = @Id;
                COMMIT TRANSACTION;
                """;
            return connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id }, cancellationToken: token));
        }, ct);

    /// <summary>See the remark on <c>ModuleRepository.ModuleRow</c> — init-only properties, no primary constructor.</summary>
    private record AlbumRow
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Slug { get; init; } = string.Empty;
        public DateOnly? EventDate { get; init; }
        public string? Description { get; init; }
        public Guid? CoverImageId { get; init; }
        public bool IsPublished { get; init; }
        public DateTimeOffset CreatedAtUtc { get; init; }

        public GalleryAlbum ToEntity() => GalleryAlbum.Rehydrate(Id, Title, Slug, EventDate, Description, CoverImageId, IsPublished, CreatedAtUtc);

        public static AlbumRow FromEntity(GalleryAlbum a) => new()
        {
            Id = a.Id,
            Title = a.Title,
            Slug = a.Slug,
            EventDate = a.EventDate,
            Description = a.Description,
            CoverImageId = a.CoverImageId,
            IsPublished = a.IsPublished,
            CreatedAtUtc = a.CreatedAtUtc,
        };
    }

    private sealed record AlbumSummaryRow : AlbumRow
    {
        public int ImageCount { get; init; }
        public Guid? EffectiveCoverId { get; init; }
    }

    private sealed record ImageRow
    {
        public Guid Id { get; init; }
        public Guid AlbumId { get; init; }
        public string? Caption { get; init; }
        public int Width { get; init; }
        public int Height { get; init; }
        public long SizeBytes { get; init; }
        public DateTimeOffset CreatedAtUtc { get; init; }

        public GalleryImage ToEntity() => GalleryImage.Rehydrate(Id, AlbumId, Caption, Width, Height, SizeBytes, CreatedAtUtc);

        public static ImageRow FromEntity(GalleryImage i) => new()
        {
            Id = i.Id,
            AlbumId = i.AlbumId,
            Caption = i.Caption,
            Width = i.Width,
            Height = i.Height,
            SizeBytes = i.SizeBytes,
            CreatedAtUtc = i.CreatedAtUtc,
        };
    }
}
