using DjMrkos.Domain.Events;

namespace DjMrkos.Application.Common.Interfaces;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Event?> GetByQrTokenAsync(string qrToken, CancellationToken ct);
    Task<IReadOnlyList<Event>> GetUpcomingAsync(CancellationToken ct);
    Task<Event> AddAsync(Event @event, CancellationToken ct);
    Task UpdateAsync(Event @event, CancellationToken ct);
}
