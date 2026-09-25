using DjMrkos.Api.Security;
using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Application.Events.Commands;
using DjMrkos.Application.Events.Dtos;
using DjMrkos.Application.Events.Queries;
using MediatR;
using Microsoft.Extensions.Options;

namespace DjMrkos.Api.Endpoints;

public static class EventsEndpoints
{
    public static void MapEventsEndpoints(this IEndpointRouteBuilder app)
    {
        // Public: the QR landing page calls this the instant a guest's phone opens it.
        app.MapGet("/api/events/qr/{token}", async (string token, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetEventByQrTokenQuery(token), ct)))
            .WithTags("Events")
            .AllowAnonymous();

        // Public: powers the /agendar availability calendar — dates only, never client details.
        app.MapGet("/api/availability", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetBusyDatesQuery(), ct)))
            .WithTags("Events")
            .AllowAnonymous();

        var admin = app.MapGroup("/api/admin/events")
            .WithTags("Admin · Events")
            .RequireAuthorization(ApiKeyAuthenticationHandler.SchemeName);

        admin.MapGet("/", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetUpcomingEventsQuery(), ct)));

        admin.MapPost("/", async (
            CreateEventCommand command,
            ISender sender,
            IQrTokenService qrTokenService,
            IOptions<FrontendOptions> frontendOptions,
            CancellationToken ct) =>
        {
            var created = await sender.Send(command, ct);
            var qrCodeDataUrl = qrTokenService.GenerateQrCodeDataUrl(created.QrToken, frontendOptions.Value.PublicBaseUrl);
            return Results.Created($"/api/admin/events/{created.Id}", new EventWithQrResponse(created, qrCodeDataUrl));
        });
    }

    public sealed record EventWithQrResponse(EventDto Event, string QrCodeDataUrl);
}

public sealed class FrontendOptions
{
    public const string SectionName = "Frontend";

    /// <summary>Base URL of the deployed React app — QR codes point guests at "{this}/evento/{token}".</summary>
    public string PublicBaseUrl { get; init; } = "http://localhost:5173";
}
