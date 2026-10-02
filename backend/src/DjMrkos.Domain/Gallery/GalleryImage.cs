using DjMrkos.Domain.Common;

namespace DjMrkos.Domain.Gallery;

/// <summary>
/// One photo of a <see cref="GalleryAlbum"/>. Only metadata lives in the database; the JPEG files
/// (a large version and a thumbnail, both already resized by the admin's browser) are on disk,
/// named after <see cref="Entity.Id"/>.
/// </summary>
public sealed class GalleryImage : Entity
{
    public Guid AlbumId { get; private set; }
    public string? Caption { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public long SizeBytes { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private GalleryImage() { }

    public static GalleryImage Create(Guid albumId, string? caption, int width, int height, long sizeBytes)
    {
        if (albumId == Guid.Empty)
            throw new DomainException("La foto debe pertenecer a un álbum.");
        if (width <= 0 || height <= 0)
            throw new DomainException("No se pudieron leer las dimensiones de la foto.");

        return new GalleryImage
        {
            AlbumId = albumId,
            Caption = string.IsNullOrWhiteSpace(caption) ? null : caption.Trim(),
            Width = width,
            Height = height,
            SizeBytes = sizeBytes,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };
    }

    public static GalleryImage Rehydrate(Guid id, Guid albumId, string? caption, int width, int height, long sizeBytes, DateTimeOffset createdAtUtc)
        => new()
        {
            Id = id,
            AlbumId = albumId,
            Caption = caption,
            Width = width,
            Height = height,
            SizeBytes = sizeBytes,
            CreatedAtUtc = createdAtUtc,
        };
}
