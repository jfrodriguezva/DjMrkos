namespace DjMrkos.Application.Common.Exceptions;

/// <summary>
/// The guest scanned a QR whose event hasn't started yet or has already finished.
/// Maps to HTTP 410 (Gone) at the API boundary — the link isn't wrong, it's just expired.
/// </summary>
public sealed class QrWindowClosedException(Guid eventId)
    : Exception($"El código QR del evento '{eventId}' no está activo en este momento.");
