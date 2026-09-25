using DjMrkos.Domain.Common;

namespace DjMrkos.Domain.SongRequests;

/// <summary>A song a guest asked for by scanning the event's QR code.</summary>
public sealed class SongRequest : Entity
{
    public Guid EventId { get; private set; }
    public string SongTitle { get; private set; } = string.Empty;
    public string? Artist { get; private set; }
    public string? RequesterName { get; private set; }
    public string? Dedication { get; private set; }
    public string RequesterFingerprint { get; private set; } = string.Empty;
    public SongRequestStatus Status { get; private set; } = SongRequestStatus.Pending;
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private SongRequest() { }

    /// <param name="requesterFingerprint">
    /// A hash identifying the guest's device (not their identity) — the only thing the
    /// rate limiter uses to stop one phone from flooding the queue.
    /// </param>
    public static SongRequest Create(Guid eventId, string songTitle, string? artist, string? requesterName, string? dedication, string requesterFingerprint)
    {
        if (eventId == Guid.Empty)
            throw new DomainException("La solicitud debe pertenecer a un evento.");
        if (string.IsNullOrWhiteSpace(songTitle))
            throw new DomainException("Escribe el nombre de la canción.");

        return new SongRequest
        {
            EventId = eventId,
            SongTitle = songTitle.Trim(),
            Artist = string.IsNullOrWhiteSpace(artist) ? null : artist.Trim(),
            RequesterName = string.IsNullOrWhiteSpace(requesterName) ? null : requesterName.Trim(),
            Dedication = string.IsNullOrWhiteSpace(dedication) ? null : dedication.Trim(),
            RequesterFingerprint = requesterFingerprint,
            Status = SongRequestStatus.Pending,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };
    }

    public static SongRequest Rehydrate(
        Guid id, Guid eventId, string songTitle, string? artist, string? requesterName,
        string? dedication, string requesterFingerprint, SongRequestStatus status, DateTimeOffset createdAtUtc)
        => new()
        {
            Id = id,
            EventId = eventId,
            SongTitle = songTitle,
            Artist = artist,
            RequesterName = requesterName,
            Dedication = dedication,
            RequesterFingerprint = requesterFingerprint,
            Status = status,
            CreatedAtUtc = createdAtUtc,
        };

    public void MoveToQueue() => Status = SongRequestStatus.Queued;

    public void MarkPlayed() => Status = SongRequestStatus.Played;

    public void Reject() => Status = SongRequestStatus.Rejected;
}
