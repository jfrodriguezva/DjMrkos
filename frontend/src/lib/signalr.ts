import * as signalR from '@microsoft/signalr'

/**
 * One connection to the song-request hub, shared by whichever admin dashboard page
 * mounts it. Auto-reconnects — a DJ's phone loses wifi mid-set more often than not.
 */
export function createSongRequestConnection(): signalR.HubConnection {
  return new signalR.HubConnectionBuilder()
    .withUrl('/hubs/song-requests')
    .withAutomaticReconnect([0, 2000, 5000, 10000, 15000])
    .configureLogging(signalR.LogLevel.Warning)
    .build()
}
