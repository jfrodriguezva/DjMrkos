using DjMrkos.Domain.Availability;

namespace DjMrkos.Application.Common.Interfaces;

public interface IBlockedDateRepository
{
    Task<BlockedDate?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>Every blocked date on or after <paramref name="from"/>, ordered by date.</summary>
    Task<IReadOnlyList<BlockedDate>> GetFromAsync(DateOnly from, CancellationToken ct);

    /// <summary>Which of <paramref name="dates"/> are already blocked — lets a range skip duplicates.</summary>
    Task<IReadOnlySet<DateOnly>> GetExistingAsync(IReadOnlyCollection<DateOnly> dates, CancellationToken ct);

    Task AddRangeAsync(IReadOnlyCollection<BlockedDate> blockedDates, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}
