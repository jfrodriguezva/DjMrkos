namespace DjMrkos.Application.Common.Interfaces;

/// <summary>
/// Best-effort alert to the DJ's phone for a new song request — independent of whether the
/// admin dashboard (and its SignalR live queue) is even open. This is the piece that actually
/// gets the DJ's attention during a live event, not just the dashboard update.
/// Implementations must never let a delivery failure bubble up: a guest's song request must
/// succeed even if the alert channel is down, rate-limited, or simply not configured yet.
/// </summary>
public interface IDjAlertNotifier
{
    Task NotifyNewSongRequestAsync(string songTitle, string? artist, string? requesterName, CancellationToken ct);
}
