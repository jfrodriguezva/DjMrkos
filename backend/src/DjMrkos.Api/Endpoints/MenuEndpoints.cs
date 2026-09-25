using DjMrkos.Application.Menu.Queries;
using MediatR;

namespace DjMrkos.Api.Endpoints;

public static class MenuEndpoints
{
    public static void MapMenuEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/menu", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetPublicMenuQuery(), ct)))
            .WithName("GetPublicMenu")
            .WithTags("Menu")
            .Produces(StatusCodes.Status200OK)
            .AllowAnonymous();
    }
}
