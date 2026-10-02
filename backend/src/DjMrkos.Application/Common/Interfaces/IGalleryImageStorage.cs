namespace DjMrkos.Application.Common.Interfaces;

public enum GalleryImageSize
{
    Large,
    Thumbnail,
}

/// <summary>Where gallery JPEGs live — a Docker volume in production, a local folder in development.</summary>
public interface IGalleryImageStorage
{
    Task SaveAsync(Guid imageId, Stream large, Stream thumbnail, CancellationToken ct);

    /// <summary>Full path of the file, or <c>null</c> if it doesn't exist.</summary>
    string? GetPath(Guid imageId, GalleryImageSize size);

    /// <summary>Removes both files of the image; missing files are not an error.</summary>
    void Delete(Guid imageId);
}
