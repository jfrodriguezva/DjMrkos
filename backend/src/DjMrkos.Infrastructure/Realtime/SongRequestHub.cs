using Microsoft.AspNetCore.SignalR;

namespace DjMrkos.Infrastructure.Realtime;

/// <summary>
/// The DJ's dashboard connects here and joins the group for the event it's watching.
/// Guests never connect to this hub — they only ever POST a song request over HTTP.
/// </summary>
public sealed class SongRequestHub : Hub
{
    public Task JoinEventGroup(Guid eventId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, GroupName(eventId));

    public Task LeaveEventGroup(Guid eventId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(eventId));

    internal static string GroupName(Guid eventId) => $"event:{eventId}";
}
