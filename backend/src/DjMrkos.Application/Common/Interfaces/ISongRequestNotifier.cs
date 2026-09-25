using DjMrkos.Application.SongRequests.Dtos;

namespace DjMrkos.Application.Common.Interfaces;

/// <summary>
/// Pushes a song-request change to whoever is watching that event's live queue — the DJ's
/// dashboard. Application depends only on this interface; Infrastructure supplies the
/// SignalR implementation, which is exactly the point of Dependency Inversion here.
/// </summary>
public interface ISongRequestNotifier
{
    Task NotifyRequestCreatedAsync(Guid eventId, SongRequestDto request, CancellationToken ct);
    Task NotifyRequestUpdatedAsync(Guid eventId, SongRequestDto request, CancellationToken ct);
}
