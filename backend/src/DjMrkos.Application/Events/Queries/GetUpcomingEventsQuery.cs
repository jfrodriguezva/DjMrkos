using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Application.Events.Dtos;
using MediatR;

namespace DjMrkos.Application.Events.Queries;

/// <summary>Admin listing, used to pick which event's live queue to watch.</summary>
public sealed record GetUpcomingEventsQuery : IRequest<IReadOnlyList<EventDto>>;

public sealed class GetUpcomingEventsQueryHandler(IEventRepository repository) : IRequestHandler<GetUpcomingEventsQuery, IReadOnlyList<EventDto>>
{
    public async Task<IReadOnlyList<EventDto>> Handle(GetUpcomingEventsQuery request, CancellationToken cancellationToken)
    {
        var events = await repository.GetUpcomingAsync(cancellationToken);
        return events.OrderBy(e => e.EventDateUtc).Select(EventDto.From).ToList();
    }
}
