using DjMrkos.Domain.Common;

namespace DjMrkos.Domain.Availability;

/// <summary>
/// A day the DJ closes on the public calendar by hand (vacation, a personal commitment) — on top
/// of the days an event already occupies. <see cref="Reason"/> is for the admin only; the public
/// availability endpoint returns bare dates.
/// </summary>
public sealed class BlockedDate : Entity
{
    public DateOnly Date { get; private set; }
    public string? Reason { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private BlockedDate() { }

    public static BlockedDate Create(DateOnly date, string? reason) => new()
    {
        Date = date,
        Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
        CreatedAtUtc = DateTimeOffset.UtcNow,
    };

    public static BlockedDate Rehydrate(Guid id, DateOnly date, string? reason, DateTimeOffset createdAtUtc) => new()
    {
        Id = id,
        Date = date,
        Reason = reason,
        CreatedAtUtc = createdAtUtc,
    };
}
