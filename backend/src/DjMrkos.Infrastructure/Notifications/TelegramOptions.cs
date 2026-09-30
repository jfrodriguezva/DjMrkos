namespace DjMrkos.Infrastructure.Notifications;

public sealed class TelegramOptions
{
    public const string SectionName = "Telegram";

    /// <summary>Token of the bot created via @BotFather. Empty means the DJ hasn't set this up — alerts are silently skipped.</summary>
    public string BotToken { get; init; } = string.Empty;

    /// <summary>Chat id the bot sends to — the DJ's own chat with the bot, or a group they're in. Free, no phone number shared with the guest.</summary>
    public string ChatId { get; init; } = string.Empty;
}
