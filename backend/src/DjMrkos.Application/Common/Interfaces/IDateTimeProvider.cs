namespace DjMrkos.Application.Common.Interfaces;

/// <summary>Abstracts "now" so QR-window and rate-limit logic is deterministic in tests.</summary>
public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
