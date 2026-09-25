using DjMrkos.Api.Security;
using DjMrkos.Application.Categories.Commands;
using DjMrkos.Application.Categories.Queries;
using MediatR;

namespace DjMrkos.Api.Endpoints;

public static class CategoriesEndpoints
{
    public static void MapCategoriesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/modules/{moduleId:guid}/categories", async (Guid moduleId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetCategoriesByModuleQuery(moduleId), ct)))
            .WithTags("Menu")
            .AllowAnonymous();

        var admin = app.MapGroup("/api/admin/categories")
            .WithTags("Admin · Categories")
            .RequireAuthorization(ApiKeyAuthenticationHandler.SchemeName);

        admin.MapPost("/", async (CreateCategoryCommand command, ISender sender, CancellationToken ct) =>
        {
            var created = await sender.Send(command, ct);
            return Results.Created($"/api/admin/categories/{created.Id}", created);
        });

        admin.MapPut("/{id:guid}", async (Guid id, UpdateCategoryRequest body, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(
                new UpdateCategoryCommand(id, body.Name, body.Description, body.ImageUrl, body.Price, body.DisplayOrder, body.IsActive), ct)));

        admin.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new DeleteCategoryCommand(id), ct);
            return Results.NoContent();
        });
    }

    public sealed record UpdateCategoryRequest(string Name, string? Description, string? ImageUrl, decimal? Price, int DisplayOrder, bool IsActive);
}
