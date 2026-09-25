using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Application.SongRequests.Dtos;
using Microsoft.AspNetCore.SignalR;

namespace DjMrkos.Infrastructure.Realtime;

public sealed class SignalRSongRequestNotifier(IHubContext<SongRequestHub> hub) : ISongRequestNotifier
{
    public Task NotifyRequestCreatedAsync(Guid eventId, SongRequestDto request, CancellationToken ct) =>
        hub.Clients.Group(SongRequestHub.GroupName(eventId)).SendAsync("songRequestCreated", request, ct);

    public Task NotifyRequestUpdatedAsync(Guid eventId, SongRequestDto request, CancellationToken ct) =>
        hub.Clients.Group(SongRequestHub.GroupName(eventId)).SendAsync("songRequestUpdated", request, ct);
}
