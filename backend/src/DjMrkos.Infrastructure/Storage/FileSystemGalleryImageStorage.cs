using DjMrkos.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace DjMrkos.Infrastructure.Storage;

public sealed class GalleryOptions
{
    public const string SectionName = "Gallery";

    /// <summary>
    /// Folder for the photo files. Relative paths resolve against the working directory
    /// (the API project in development); production mounts a Docker volume at an absolute path.
    /// </summary>
    public string StoragePath { get; init; } = "App_Data/gallery";
}

/// <summary>Two files per photo: <c>{id}.jpg</c> (large) and <c>{id}_thumb.jpg</c>.</summary>
public sealed class FileSystemGalleryImageStorage : IGalleryImageStorage
{
    private readonly string _root;

    public FileSystemGalleryImageStorage(IOptions<GalleryOptions> options)
    {
        _root = Path.GetFullPath(options.Value.StoragePath);
        Directory.CreateDirectory(_root);
    }

    public async Task SaveAsync(Guid imageId, Stream large, Stream thumbnail, CancellationToken ct)
    {
        await WriteAtomicallyAsync(PathFor(imageId, GalleryImageSize.Large), large, ct);
        try
        {
            await WriteAtomicallyAsync(PathFor(imageId, GalleryImageSize.Thumbnail), thumbnail, ct);
        }
        catch
        {
            Delete(imageId);
            throw;
        }
    }

    public string? GetPath(Guid imageId, GalleryImageSize size)
    {
        var path = PathFor(imageId, size);
        return File.Exists(path) ? path : null;
    }

    public void Delete(Guid imageId)
    {
        File.Delete(PathFor(imageId, GalleryImageSize.Large));
        File.Delete(PathFor(imageId, GalleryImageSize.Thumbnail));
    }

    // The id is a Guid, never user text, so the file name can't escape the folder.
    private string PathFor(Guid imageId, GalleryImageSize size) =>
        Path.Combine(_root, size == GalleryImageSize.Thumbnail ? $"{imageId:N}_thumb.jpg" : $"{imageId:N}.jpg");

    // Write to a temp file and rename, so a visitor never gets a half-written photo.
    private static async Task WriteAtomicallyAsync(string path, Stream content, CancellationToken ct)
    {
        var temp = path + ".tmp";
        await using (var file = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            await content.CopyToAsync(file, ct);
        File.Move(temp, path, overwrite: true);
    }
}
