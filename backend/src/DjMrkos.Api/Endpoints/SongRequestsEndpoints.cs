using System.Security.Cryptography;
using System.Text;
using DjMrkos.Api.Security;
using DjMrkos.Application.SongRequests.Commands;
using DjMrkos.Application.SongRequests.Queries;
using MediatR;

namespace DjMrkos.Api.Endpoints;

public static class SongRequestsEndpoints
{
    public static void MapSongRequestsEndpoints(this IEndpointRouteBuilder app)
    {
        // Public: what the guest's phone posts after they fill in the QR form.
        app.MapPost("/api/events/qr/{token}/song-requests", async (
            string token, CreateSongRequestBody body, HttpContext http, ISender sender, CancellationToken ct) =>
        {
            var fingerprint = ComputeFingerprint(http);
            var command = new CreateSongRequestCommand(token, body.SongTitle, body.Artist, body.RequesterName, body.Dedication, fingerprint);
            var created = await sender.Send(command, ct);
            return Results.Created($"/api/admin/song-requests/{created.Id}", created);
        })
        .WithTags("Song Requests")
        .AllowAnonymous();

        var admin = app.MapGroup("/api/admin")
            .WithTags("Admin · Song Requests")
            .RequireAuthorization(ApiKeyAuthenticationHandler.SchemeName);

        admin.MapGet("/events/{eventId:guid}/song-requests", async (Guid eventId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetSongRequestsByEventQuery(eventId), ct)));

        admin.MapPatch("/song-requests/{id:guid}/status", async (Guid id, UpdateStatusBody body, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new UpdateSongRequestStatusCommand(id, body.Action), ct)));
    }

    /// <summary>
    /// Not an identity — a coarse, non-reversible fingerprint of "this phone" so the rate
    /// limiter can tell one guest's five rapid requests from five different guests'.
    /// </summary>
    private static string ComputeFingerprint(HttpContext http)
    {
        var ip = http.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = http.Request.Headers.UserAgent.ToString();
        var bytes = Encoding.UTF8.GetBytes($"{ip}|{userAgent}");
        return Convert.ToHexString(SHA256.HashData(bytes));
    }

    public sealed record CreateSongRequestBody(string SongTitle, string? Artist, string? RequesterName, string? Dedication);

    public sealed record UpdateStatusBody(SongRequestAction Action);
}
