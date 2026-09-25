using DjMrkos.Api.Security;
using DjMrkos.Application.Modules.Commands;
using DjMrkos.Application.Modules.Queries;
using MediatR;

namespace DjMrkos.Api.Endpoints;

public static class ModulesEndpoints
{
    public static void MapModulesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/modules")
            .WithTags("Admin · Modules")
            .RequireAuthorization(ApiKeyAuthenticationHandler.SchemeName);

        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetModulesQuery(), ct)));

        group.MapPost("/", async (CreateModuleCommand command, ISender sender, CancellationToken ct) =>
        {
            var created = await sender.Send(command, ct);
            return Results.Created($"/api/admin/modules/{created.Id}", created);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateModuleRequest body, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new UpdateModuleCommand(id, body.Name, body.Icon, body.DisplayOrder, body.IsActive), ct)));

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new DeleteModuleCommand(id), ct);
            return Results.NoContent();
        });

        group.MapPost("/reorder", async (ReorderModulesCommand command, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(command, ct);
            return Results.NoContent();
        });
    }

    public sealed record UpdateModuleRequest(string Name, string? Icon, int DisplayOrder, bool IsActive);
}
