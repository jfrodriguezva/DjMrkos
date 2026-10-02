using DjMrkos.Api.Security;
using DjMrkos.Application.Availability;
using MediatR;

namespace DjMrkos.Api.Endpoints;

/// <summary>
/// Admin-only. The public side needs nothing new: GET /api/availability (EventsEndpoints)
/// already returns blocked days mixed in with event days, without the reason.
/// </summary>
public static class BlockedDatesEndpoints
{
    public static void MapBlockedDatesEndpoints(this IEndpointRouteBuilder app)
    {
        var admin = app.MapGroup("/api/admin/blocked-dates")
            .WithTags("Admin · Calendar")
            .RequireAuthorization(ApiKeyAuthenticationHandler.SchemeName);

        admin.MapGet("/", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetBlockedDatesQuery(), ct)));

        admin.MapPost("/", async (BlockDatesCommand command, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(command, ct)));

        admin.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new UnblockDateCommand(id), ct);
            return Results.NoContent();
        });
    }
}
