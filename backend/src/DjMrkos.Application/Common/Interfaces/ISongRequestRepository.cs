using DjMrkos.Domain.SongRequests;

namespace DjMrkos.Application.Common.Interfaces;

public interface ISongRequestRepository
{
    Task<SongRequest?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<SongRequest>> GetByEventIdAsync(Guid eventId, CancellationToken ct);

    /// <summary>How many requests this fingerprint has made for this event since <paramref name="sinceUtc"/> — the rate-limit check.</summary>
    Task<int> CountByFingerprintSinceAsync(Guid eventId, string fingerprint, DateTimeOffset sinceUtc, CancellationToken ct);

    Task<SongRequest> AddAsync(SongRequest songRequest, CancellationToken ct);
    Task UpdateAsync(SongRequest songRequest, CancellationToken ct);
}
