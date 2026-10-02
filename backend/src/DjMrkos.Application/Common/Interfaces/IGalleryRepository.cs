using DjMrkos.Domain.Gallery;

namespace DjMrkos.Application.Common.Interfaces;

/// <param name="CoverImageId">The chosen cover, or the album's first photo when none was chosen.</param>
public sealed record GalleryAlbumSummary(GalleryAlbum Album, int ImageCount, Guid? CoverImageId);

public interface IGalleryRepository
{
    Task<IReadOnlyList<GalleryAlbumSummary>> GetAlbumSummariesAsync(bool publishedOnly, CancellationToken ct);
    Task<GalleryAlbum?> GetAlbumByIdAsync(Guid id, CancellationToken ct);
    Task<GalleryAlbum?> GetAlbumBySlugAsync(string slug, CancellationToken ct);
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct);
    Task AddAlbumAsync(GalleryAlbum album, CancellationToken ct);
    Task UpdateAlbumAsync(GalleryAlbum album, CancellationToken ct);

    /// <summary>Deletes the album row; its image rows go with it (ON DELETE CASCADE).</summary>
    Task DeleteAlbumAsync(Guid id, CancellationToken ct);

    Task<IReadOnlyList<GalleryImage>> GetImagesAsync(Guid albumId, CancellationToken ct);
    Task<GalleryImage?> GetImageByIdAsync(Guid id, CancellationToken ct);
    Task AddImageAsync(GalleryImage image, CancellationToken ct);

    /// <summary>Deletes the image row and clears it as its album's cover if it was.</summary>
    Task DeleteImageAsync(Guid id, CancellationToken ct);
}
