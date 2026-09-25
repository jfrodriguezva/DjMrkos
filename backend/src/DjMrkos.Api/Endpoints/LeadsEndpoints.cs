using DjMrkos.Api.Security;
using DjMrkos.Application.Leads.Commands;
using DjMrkos.Application.Leads.Queries;
using MediatR;

namespace DjMrkos.Api.Endpoints;

public static class LeadsEndpoints
{
    public static void MapLeadsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/leads", async (CreateLeadCommand command, ISender sender, CancellationToken ct) =>
        {
            var created = await sender.Send(command, ct);
            return Results.Created($"/api/admin/leads/{created.Id}", created);
        })
        .WithTags("Leads")
        .AllowAnonymous();

        app.MapGet("/api/admin/leads", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetLeadsQuery(), ct)))
            .WithTags("Admin · Leads")
            .RequireAuthorization(ApiKeyAuthenticationHandler.SchemeName);
    }
}
