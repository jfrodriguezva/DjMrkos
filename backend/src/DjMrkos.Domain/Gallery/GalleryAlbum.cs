using System.Globalization;
using System.Text;
using DjMrkos.Domain.Common;

namespace DjMrkos.Domain.Gallery;

/// <summary>
/// One section of the public gallery — normally one event (a wedding, a quinceañera). Starts as a
/// draft so the DJ can upload every photo before visitors see a half-filled album.
/// </summary>
public sealed class GalleryAlbum : Entity
{
    public string Title { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public DateOnly? EventDate { get; private set; }
    public string? Description { get; private set; }
    public Guid? CoverImageId { get; private set; }
    public bool IsPublished { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private GalleryAlbum() { }

    /// <param name="slug">Already unique — callers derive it with <see cref="ToSlug"/> and de-duplicate.</param>
    public static GalleryAlbum Create(string title, string slug, DateOnly? eventDate, string? description)
    {
        Validate(title);
        return new GalleryAlbum
        {
            Title = title.Trim(),
            Slug = slug,
            EventDate = eventDate,
            Description = Normalize(description),
            IsPublished = false,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };
    }

    public static GalleryAlbum Rehydrate(
        Guid id, string title, string slug, DateOnly? eventDate, string? description, Guid? coverImageId, bool isPublished, DateTimeOffset createdAtUtc)
        => new()
        {
            Id = id,
            Title = title,
            Slug = slug,
            EventDate = eventDate,
            Description = description,
            CoverImageId = coverImageId,
            IsPublished = isPublished,
            CreatedAtUtc = createdAtUtc,
        };

    /// <summary>The slug (public URL) deliberately stays the same when the title changes, so shared links keep working.</summary>
    public void Update(string title, DateOnly? eventDate, string? description, bool isPublished)
    {
        Validate(title);
        Title = title.Trim();
        EventDate = eventDate;
        Description = Normalize(description);
        IsPublished = isPublished;
    }

    public void SetCover(Guid? imageId) => CoverImageId = imageId;

    /// <summary>"Boda Ana & Luis — Cuernavaca" -> "boda-ana-luis-cuernavaca".</summary>
    public static string ToSlug(string title)
    {
        var decomposed = title.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        var lastWasDash = true;

        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                continue;
            if (c is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                builder.Append(c);
                lastWasDash = false;
            }
            else if (!lastWasDash)
            {
                builder.Append('-');
                lastWasDash = true;
            }
        }

        var slug = builder.ToString().Trim('-');
        if (slug.Length > 200)
            slug = slug[..200].Trim('-');
        return slug.Length == 0 ? "album" : slug;
    }

    private static void Validate(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("El álbum necesita un título.");
    }

    private static string? Normalize(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}
