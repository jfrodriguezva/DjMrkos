using DjMrkos.Application.Common.Exceptions;
using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Domain.Gallery;
using MediatR;

namespace DjMrkos.Application.Gallery;

/// <summary>Public list (published only) or the admin list (drafts too).</summary>
public sealed record GetGalleryAlbumsQuery(bool PublishedOnly) : IRequest<IReadOnlyList<GalleryAlbumDto>>;

public sealed class GetGalleryAlbumsQueryHandler(IGalleryRepository gallery) : IRequestHandler<GetGalleryAlbumsQuery, IReadOnlyList<GalleryAlbumDto>>
{
    public async Task<IReadOnlyList<GalleryAlbumDto>> Handle(GetGalleryAlbumsQuery request, CancellationToken cancellationToken)
    {
        var summaries = await gallery.GetAlbumSummariesAsync(request.PublishedOnly, cancellationToken);

        // A published album with no photos yet would be an empty card on the public page.
        return summaries
            .Where(s => !request.PublishedOnly || s.ImageCount > 0)
            .Select(GalleryAlbumDto.From)
            .ToList();
    }
}

/// <summary>Public album page. Drafts answer 404, the same as an album that doesn't exist.</summary>
public sealed record GetPublishedGalleryAlbumQuery(string Slug) : IRequest<GalleryAlbumDetailDto>;

public sealed class GetPublishedGalleryAlbumQueryHandler(IGalleryRepository gallery) : IRequestHandler<GetPublishedGalleryAlbumQuery, GalleryAlbumDetailDto>
{
    public async Task<GalleryAlbumDetailDto> Handle(GetPublishedGalleryAlbumQuery request, CancellationToken cancellationToken)
    {
        var album = await gallery.GetAlbumBySlugAsync(request.Slug, cancellationToken);
        if (album is null || !album.IsPublished)
            throw new NotFoundException(nameof(GalleryAlbum), request.Slug);

        var images = await gallery.GetImagesAsync(album.Id, cancellationToken);
        return GalleryAlbumDetailDto.From(album, images);
    }
}

/// <summary>Admin album editor — drafts included.</summary>
public sealed record GetGalleryAlbumQuery(Guid Id) : IRequest<GalleryAlbumDetailDto>;

public sealed class GetGalleryAlbumQueryHandler(IGalleryRepository gallery) : IRequestHandler<GetGalleryAlbumQuery, GalleryAlbumDetailDto>
{
    public async Task<GalleryAlbumDetailDto> Handle(GetGalleryAlbumQuery request, CancellationToken cancellationToken)
    {
        var album = await gallery.GetAlbumByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(GalleryAlbum), request.Id);

        var images = await gallery.GetImagesAsync(album.Id, cancellationToken);
        return GalleryAlbumDetailDto.From(album, images);
    }
}
