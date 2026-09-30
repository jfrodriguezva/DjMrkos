using System.Net.Http.Json;
using DjMrkos.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DjMrkos.Infrastructure.Notifications;

/// <summary>
/// Sends the DJ a Telegram message when a guest requests a song — a free push notification
/// to their phone (via the Telegram app they already have) that works even if the admin
/// dashboard tab is closed. Setup is just creating a bot with @BotFather and starting a chat
/// with it; no paid SMS/push service, no card on file. If <see cref="TelegramOptions"/> isn't
/// configured yet, this is a silent no-op — the song request itself must never fail because
/// of this.
/// </summary>
public sealed class TelegramDjAlertNotifier(HttpClient httpClient, IOptions<TelegramOptions> options, ILogger<TelegramDjAlertNotifier> logger)
    : IDjAlertNotifier
{
    public async Task NotifyNewSongRequestAsync(string songTitle, string? artist, string? requesterName, CancellationToken ct)
    {
        var botToken = options.Value.BotToken;
        var chatId = options.Value.ChatId;
        if (string.IsNullOrWhiteSpace(botToken) || string.IsNullOrWhiteSpace(chatId))
            return;

        var text = $"Nueva canción pedida: {songTitle}"
            + (string.IsNullOrWhiteSpace(artist) ? "" : $" — {artist}")
            + (string.IsNullOrWhiteSpace(requesterName) ? "" : $"\nPedida por: {requesterName}");

        try
        {
            var response = await httpClient.PostAsJsonAsync(
                $"https://api.telegram.org/bot{botToken}/sendMessage",
                new { chat_id = chatId, text },
                ct);

            if (!response.IsSuccessStatusCode)
                logger.LogWarning("Telegram respondió {StatusCode} al notificar una solicitud de canción.", response.StatusCode);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "No se pudo enviar la alerta de Telegram al DJ.");
        }
    }
}
