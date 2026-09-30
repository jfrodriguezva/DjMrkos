using DjMrkos.Api.Security;
using DjMrkos.Application.Promotions.Commands;
using DjMrkos.Application.Promotions.Queries;
using MediatR;

namespace DjMrkos.Api.Endpoints;

public static class PromotionsEndpoints
{
    public static void MapPromotionsEndpoints(this IEndpointRouteBuilder app)
    {
        var admin = app.MapGroup("/api/admin/promotions")
            .WithTags("Admin · Promotions")
            .RequireAuthorization(ApiKeyAuthenticationHandler.SchemeName);

        admin.MapGet("/", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetPromotionsQuery(), ct)));

        admin.MapPost("/", async (CreatePromotionCommand command, ISender sender, CancellationToken ct) =>
        {
            var created = await sender.Send(command, ct);
            return Results.Created($"/api/admin/promotions/{created.Id}", created);
        });

        admin.MapPut("/{id:guid}", async (Guid id, UpdatePromotionRequest body, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new UpdatePromotionCommand(id, body.Label, body.DiscountPercentage, body.IsActive), ct)));

        admin.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new DeletePromotionCommand(id), ct);
            return Results.NoContent();
        });
    }

    public sealed record UpdatePromotionRequest(string Label, decimal DiscountPercentage, bool IsActive);
}
