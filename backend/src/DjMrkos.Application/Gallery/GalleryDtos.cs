using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Domain.Gallery;

namespace DjMrkos.Application.Gallery;

/// <param name="CoverImageId">The chosen cover, or the first photo when none was chosen; null for an empty album.</param>
public sealed record GalleryAlbumDto(
    Guid Id,
    string Title,
    string Slug,
    DateOnly? EventDate,
    string? Description,
    Guid? CoverImageId,
    bool IsPublished,
    int ImageCount,
    DateTimeOffset CreatedAtUtc)
{
    public static GalleryAlbumDto From(GalleryAlbumSummary summary) => From(summary.Album, summary.ImageCount, summary.CoverImageId);

    public static GalleryAlbumDto From(GalleryAlbum album, int imageCount, Guid? coverImageId) => new(
        album.Id, album.Title, album.Slug, album.EventDate, album.Description, coverImageId, album.IsPublished, imageCount, album.CreatedAtUtc);
}

public sealed record GalleryImageDto(Guid Id, Guid AlbumId, string? Caption, int Width, int Height, DateTimeOffset CreatedAtUtc)
{
    public static GalleryImageDto From(GalleryImage image) => new(image.Id, image.AlbumId, image.Caption, image.Width, image.Height, image.CreatedAtUtc);
}

public sealed record GalleryAlbumDetailDto(GalleryAlbumDto Album, IReadOnlyList<GalleryImageDto> Images)
{
    public static GalleryAlbumDetailDto From(GalleryAlbum album, IReadOnlyList<GalleryImage> images)
    {
        var cover = album.CoverImageId ?? images.FirstOrDefault()?.Id;
        return new(GalleryAlbumDto.From(album, images.Count, cover), images.Select(GalleryImageDto.From).ToList());
    }
}
