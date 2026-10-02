using DjMrkos.Application.Common.Interfaces;
using MediatR;

namespace DjMrkos.Application.Events.Queries;

/// <summary>
/// Feeds the public availability calendar (/agendar) — every date with at least one
/// non-cancelled event on it, plus the days the DJ blocked by hand. Deliberately returns bare
/// dates, never client names, event details or the private reason for a block, since this is
/// called by anonymous visitors before they've contracted anything.
/// </summary>
public sealed record GetBusyDatesQuery : IRequest<IReadOnlyList<DateOnly>>;

public sealed class GetBusyDatesQueryHandler(IEventRepository events, IBlockedDateRepository blockedDates, IDateTimeProvider clock)
    : IRequestHandler<GetBusyDatesQuery, IReadOnlyList<DateOnly>>
{
    public async Task<IReadOnlyList<DateOnly>> Handle(GetBusyDatesQuery request, CancellationToken cancellationToken)
    {
        var eventDates = (await events.GetUpcomingAsync(cancellationToken))
            .Select(e => DateOnly.FromDateTime(e.EventDateUtc.UtcDateTime));

        var yesterday = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime).AddDays(-1);
        var blocked = (await blockedDates.GetFromAsync(yesterday, cancellationToken)).Select(b => b.Date);

        return eventDates.Concat(blocked).Distinct().OrderBy(d => d).ToList();
    }
}
