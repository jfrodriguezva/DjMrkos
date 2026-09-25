using DjMrkos.Domain.Common;

namespace DjMrkos.Domain.Events;

/// <summary>
/// A contracted gig. Each event owns exactly one QR song-request window: the
/// token is only valid between <see cref="QrValidFromUtc"/> and <see cref="QrValidUntilUtc"/>,
/// so a guest can never resurrect a QR code printed for a party that already ended.
/// </summary>
public sealed class Event : Entity
{
    public string ClientName { get; private set; } = string.Empty;
    public string? Location { get; private set; }
    public DateTimeOffset EventDateUtc { get; private set; }
    public EventStatus Status { get; private set; } = EventStatus.Scheduled;

    public string QrToken { get; private set; } = string.Empty;
    public DateTimeOffset QrValidFromUtc { get; private set; }
    public DateTimeOffset QrValidUntilUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    private Event() { }

    /// <summary>
    /// Creates a scheduled event with its <see cref="Id"/> already assigned but no QR token yet —
    /// the token embeds this event's id, so it can only be minted once the id exists. Call
    /// <see cref="IssueQrToken"/> right after with a token from <c>IQrTokenService</c>.
    /// The QR window defaults to opening two hours before the event starts and closing six
    /// hours after — wide enough to cover setup and encores, narrow enough that a guest
    /// can't submit requests days later.
    /// </summary>
    public static Event Schedule(string clientName, string? location, DateTimeOffset eventDateUtc)
    {
        if (string.IsNullOrWhiteSpace(clientName))
            throw new DomainException("El evento necesita el nombre del cliente.");

        return new Event
        {
            ClientName = clientName.Trim(),
            Location = location,
            EventDateUtc = eventDateUtc,
            Status = EventStatus.Scheduled,
            QrValidFromUtc = eventDateUtc.AddHours(-2),
            QrValidUntilUtc = eventDateUtc.AddHours(6),
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };
    }

    public void IssueQrToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new DomainException("El token de QR no puede estar vacío.");

        QrToken = token;
    }

    public static Event Rehydrate(
        Guid id, string clientName, string? location, DateTimeOffset eventDateUtc, EventStatus status,
        string qrToken, DateTimeOffset qrValidFromUtc, DateTimeOffset qrValidUntilUtc, DateTimeOffset createdAtUtc)
        => new()
        {
            Id = id,
            ClientName = clientName,
            Location = location,
            EventDateUtc = eventDateUtc,
            Status = status,
            QrToken = qrToken,
            QrValidFromUtc = qrValidFromUtc,
            QrValidUntilUtc = qrValidUntilUtc,
            CreatedAtUtc = createdAtUtc,
        };

    public bool IsQrWindowOpen(DateTimeOffset atUtc) =>
        Status is EventStatus.Scheduled or EventStatus.Live &&
        atUtc >= QrValidFromUtc &&
        atUtc <= QrValidUntilUtc;

    public void GoLive() => Status = EventStatus.Live;

    public void Complete() => Status = EventStatus.Completed;

    public void Cancel() => Status = EventStatus.Cancelled;
}
