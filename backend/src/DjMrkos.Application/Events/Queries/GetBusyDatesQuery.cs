using DjMrkos.Application.Common.Interfaces;
using MediatR;

namespace DjMrkos.Application.Events.Queries;

/// <summary>
/// Feeds the public availability calendar (/agendar) — every date with at least one
/// non-cancelled event on it. Deliberately returns bare dates, never client names or event
/// details, since this is called by anonymous visitors before they've contracted anything.
/// </summary>
public sealed record GetBusyDatesQuery : IRequest<IReadOnlyList<DateOnly>>;

public sealed class GetBusyDatesQueryHandler(IEventRepository repository) : IRequestHandler<GetBusyDatesQuery, IReadOnlyList<DateOnly>>
{
    public async Task<IReadOnlyList<DateOnly>> Handle(GetBusyDatesQuery request, CancellationToken cancellationToken)
    {
        var events = await repository.GetUpcomingAsync(cancellationToken);
        return events
            .Select(e => DateOnly.FromDateTime(e.EventDateUtc.UtcDateTime))
            .Distinct()
            .OrderBy(d => d)
            .ToList();
    }
}
