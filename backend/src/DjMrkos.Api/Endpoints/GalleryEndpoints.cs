using DjMrkos.Api.Security;
using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Application.Gallery;
using MediatR;

namespace DjMrkos.Api.Endpoints;

public static class GalleryEndpoints
{
    public static void MapGalleryEndpoints(this IEndpointRouteBuilder app)
    {
        var publicGroup = app.MapGroup("/api/gallery").WithTags("Gallery").AllowAnonymous();

        publicGroup.MapGet("/albums", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetGalleryAlbumsQuery(PublishedOnly: true), ct)));

        publicGroup.MapGet("/albums/{slug}", async (string slug, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetPublishedGalleryAlbumQuery(slug), ct)));

        // Served by unguessable id, so the admin editor can preview photos of draft albums with a
        // plain <img> (which can't send the X-Api-Key header). A photo's bytes never change for a
        // given id, so browsers may cache it forever.
        publicGroup.MapGet("/images/{id:guid}/{size}", (Guid id, string size, IGalleryImageStorage storage, HttpContext http) =>
        {
            GalleryImageSize? parsed = size switch
            {
                "large" => GalleryImageSize.Large,
                "thumb" => GalleryImageSize.Thumbnail,
                _ => null,
            };
            if (parsed is null)
                return Results.NotFound();

            var path = storage.GetPath(id, parsed.Value);
            if (path is null)
                return Results.NotFound();

            http.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
            return Results.File(path, "image/jpeg");
        });

        var admin = app.MapGroup("/api/admin/gallery")
            .WithTags("Admin · Gallery")
            .RequireAuthorization(ApiKeyAuthenticationHandler.SchemeName);

        admin.MapGet("/albums", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetGalleryAlbumsQuery(PublishedOnly: false), ct)));

        admin.MapGet("/albums/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetGalleryAlbumQuery(id), ct)));

        admin.MapPost("/albums", async (CreateGalleryAlbumCommand command, ISender sender, CancellationToken ct) =>
        {
            var created = await sender.Send(command, ct);
            return Results.Created($"/api/admin/gallery/albums/{created.Id}", created);
        });

        admin.MapPut("/albums/{id:guid}", async (Guid id, UpdateAlbumRequest body, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new UpdateGalleryAlbumCommand(id, body.Title, body.EventDate, body.Description, body.IsPublished), ct)));

        admin.MapPut("/albums/{id:guid}/cover", async (Guid id, SetCoverRequest body, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new SetGalleryAlbumCoverCommand(id, body.ImageId), ct);
            return Results.NoContent();
        });

        admin.MapDelete("/albums/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new DeleteGalleryAlbumCommand(id), ct);
            return Results.NoContent();
        });

        // One photo per request (the panel uploads a batch one by one, with progress), as
        // multipart/form-data: "image" + "thumbnail" JPEGs, "width", "height", optional "caption".
        // The form is read by hand rather than bound, so no antiforgery metadata is attached —
        // this endpoint is authenticated by the API key header, not by cookies.
        admin.MapPost("/albums/{id:guid}/images", async (Guid id, HttpRequest request, ISender sender, CancellationToken ct) =>
        {
            if (!request.HasFormContentType)
                return Results.Problem(title: "Envía la foto como multipart/form-data.", statusCode: StatusCodes.Status400BadRequest);

            var form = await request.ReadFormAsync(ct);
            var image = form.Files.GetFile("image");
            var thumbnail = form.Files.GetFile("thumbnail");
            if (image is null || thumbnail is null)
                return Results.Problem(title: "Faltan la foto o su miniatura.", statusCode: StatusCodes.Status400BadRequest);

            _ = int.TryParse(form["width"], out var width);
            _ = int.TryParse(form["height"], out var height);

            var created = await sender.Send(new AddGalleryImageCommand(
                id, form["caption"].ToString(), width, height, await ReadAllAsync(image, ct), await ReadAllAsync(thumbnail, ct)), ct);
            return Results.Created($"/api/gallery/images/{created.Id}/large", created);
        });

        admin.MapDelete("/images/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new DeleteGalleryImageCommand(id), ct);
            return Results.NoContent();
        });
    }

    private static async Task<byte[]> ReadAllAsync(IFormFile file, CancellationToken ct)
    {
        // Size is re-checked by AddGalleryImageCommandValidator; this cap just avoids buffering something absurd.
        if (file.Length > AddGalleryImageCommandValidator.MaxLargeBytes)
            return [];

        using var buffer = new MemoryStream((int)file.Length);
        await file.CopyToAsync(buffer, ct);
        return buffer.ToArray();
    }

    public sealed record UpdateAlbumRequest(string Title, DateOnly? EventDate, string? Description, bool IsPublished);

    public sealed record SetCoverRequest(Guid? ImageId);
}
