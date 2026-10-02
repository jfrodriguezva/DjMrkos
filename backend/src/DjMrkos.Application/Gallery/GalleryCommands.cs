using DjMrkos.Application.Common.Exceptions;
using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Domain.Common;
using DjMrkos.Domain.Gallery;
using FluentValidation;
using MediatR;

namespace DjMrkos.Application.Gallery;

public sealed record CreateGalleryAlbumCommand(string Title, DateOnly? EventDate, string? Description) : IRequest<GalleryAlbumDto>;

public sealed class CreateGalleryAlbumCommandValidator : AbstractValidator<CreateGalleryAlbumCommand>
{
    public CreateGalleryAlbumCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public sealed class CreateGalleryAlbumCommandHandler(IGalleryRepository gallery) : IRequestHandler<CreateGalleryAlbumCommand, GalleryAlbumDto>
{
    public async Task<GalleryAlbumDto> Handle(CreateGalleryAlbumCommand request, CancellationToken cancellationToken)
    {
        // Two "Boda" albums get "boda" and "boda-2" — the slug is the public URL, so it must be unique.
        var baseSlug = GalleryAlbum.ToSlug(request.Title);
        var slug = baseSlug;
        for (var n = 2; await gallery.SlugExistsAsync(slug, cancellationToken); n++)
            slug = $"{baseSlug}-{n}";

        var album = GalleryAlbum.Create(request.Title, slug, request.EventDate, request.Description);
        await gallery.AddAlbumAsync(album, cancellationToken);
        return GalleryAlbumDto.From(album, imageCount: 0, coverImageId: null);
    }
}

public sealed record UpdateGalleryAlbumCommand(Guid Id, string Title, DateOnly? EventDate, string? Description, bool IsPublished)
    : IRequest<GalleryAlbumDetailDto>;

public sealed class UpdateGalleryAlbumCommandValidator : AbstractValidator<UpdateGalleryAlbumCommand>
{
    public UpdateGalleryAlbumCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public sealed class UpdateGalleryAlbumCommandHandler(IGalleryRepository gallery) : IRequestHandler<UpdateGalleryAlbumCommand, GalleryAlbumDetailDto>
{
    public async Task<GalleryAlbumDetailDto> Handle(UpdateGalleryAlbumCommand request, CancellationToken cancellationToken)
    {
        var album = await gallery.GetAlbumByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(GalleryAlbum), request.Id);

        album.Update(request.Title, request.EventDate, request.Description, request.IsPublished);
        await gallery.UpdateAlbumAsync(album, cancellationToken);
        return GalleryAlbumDetailDto.From(album, await gallery.GetImagesAsync(album.Id, cancellationToken));
    }
}

/// <summary>Picks the album's cover photo; <c>ImageId = null</c> goes back to "first photo".</summary>
public sealed record SetGalleryAlbumCoverCommand(Guid AlbumId, Guid? ImageId) : IRequest;

public sealed class SetGalleryAlbumCoverCommandHandler(IGalleryRepository gallery) : IRequestHandler<SetGalleryAlbumCoverCommand>
{
    public async Task Handle(SetGalleryAlbumCoverCommand request, CancellationToken cancellationToken)
    {
        var album = await gallery.GetAlbumByIdAsync(request.AlbumId, cancellationToken)
            ?? throw new NotFoundException(nameof(GalleryAlbum), request.AlbumId);

        if (request.ImageId is { } imageId)
        {
            var image = await gallery.GetImageByIdAsync(imageId, cancellationToken);
            if (image is null || image.AlbumId != album.Id)
                throw new DomainException("La portada debe ser una foto de este mismo álbum.");
        }

        album.SetCover(request.ImageId);
        await gallery.UpdateAlbumAsync(album, cancellationToken);
    }
}

public sealed record DeleteGalleryAlbumCommand(Guid Id) : IRequest;

public sealed class DeleteGalleryAlbumCommandHandler(IGalleryRepository gallery, IGalleryImageStorage storage) : IRequestHandler<DeleteGalleryAlbumCommand>
{
    public async Task Handle(DeleteGalleryAlbumCommand request, CancellationToken cancellationToken)
    {
        _ = await gallery.GetAlbumByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(GalleryAlbum), request.Id);

        var images = await gallery.GetImagesAsync(request.Id, cancellationToken);
        await gallery.DeleteAlbumAsync(request.Id, cancellationToken);

        // Files go only after the rows are gone: a failed DB delete must not leave rows pointing at missing files.
        foreach (var image in images)
            storage.Delete(image.Id);
    }
}

/// <summary>
/// Stores one photo. The admin's browser already resized it and produced the thumbnail (both
/// re-encoded as JPEG, which also strips EXIF/GPS data), so the server only checks and saves.
/// </summary>
public sealed record AddGalleryImageCommand(Guid AlbumId, string? Caption, int Width, int Height, byte[] Large, byte[] Thumbnail)
    : IRequest<GalleryImageDto>;

public sealed class AddGalleryImageCommandValidator : AbstractValidator<AddGalleryImageCommand>
{
    public const int MaxLargeBytes = 10 * 1024 * 1024;
    public const int MaxThumbnailBytes = 2 * 1024 * 1024;

    public AddGalleryImageCommandValidator()
    {
        RuleFor(x => x.Caption).MaximumLength(300);
        RuleFor(x => x.Width).InclusiveBetween(1, 10_000);
        RuleFor(x => x.Height).InclusiveBetween(1, 10_000);
        RuleFor(x => x.Large)
            .Must(b => b.Length is > 0 and <= MaxLargeBytes).WithMessage("La foto debe pesar menos de 10 MB.")
            .Must(IsJpeg).WithMessage("La foto debe ser JPEG.");
        RuleFor(x => x.Thumbnail)
            .Must(b => b.Length is > 0 and <= MaxThumbnailBytes).WithMessage("La miniatura debe pesar menos de 2 MB.")
            .Must(IsJpeg).WithMessage("La miniatura debe ser JPEG.");
    }

    // Check the actual bytes, not the declared Content-Type, which the client controls.
    private static bool IsJpeg(byte[] bytes) => bytes.Length > 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF;
}

public sealed class AddGalleryImageCommandHandler(IGalleryRepository gallery, IGalleryImageStorage storage) : IRequestHandler<AddGalleryImageCommand, GalleryImageDto>
{
    public async Task<GalleryImageDto> Handle(AddGalleryImageCommand request, CancellationToken cancellationToken)
    {
        _ = await gallery.GetAlbumByIdAsync(request.AlbumId, cancellationToken)
            ?? throw new NotFoundException(nameof(GalleryAlbum), request.AlbumId);

        var image = GalleryImage.Create(request.AlbumId, request.Caption, request.Width, request.Height, request.Large.LongLength);

        using (var large = new MemoryStream(request.Large, writable: false))
        using (var thumbnail = new MemoryStream(request.Thumbnail, writable: false))
            await storage.SaveAsync(image.Id, large, thumbnail, cancellationToken);

        try
        {
            await gallery.AddImageAsync(image, cancellationToken);
        }
        catch
        {
            storage.Delete(image.Id);
            throw;
        }

        return GalleryImageDto.From(image);
    }
}

public sealed record DeleteGalleryImageCommand(Guid Id) : IRequest;

public sealed class DeleteGalleryImageCommandHandler(IGalleryRepository gallery, IGalleryImageStorage storage) : IRequestHandler<DeleteGalleryImageCommand>
{
    public async Task Handle(DeleteGalleryImageCommand request, CancellationToken cancellationToken)
    {
        _ = await gallery.GetImageByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(GalleryImage), request.Id);

        await gallery.DeleteImageAsync(request.Id, cancellationToken);
        storage.Delete(request.Id);
    }
}
