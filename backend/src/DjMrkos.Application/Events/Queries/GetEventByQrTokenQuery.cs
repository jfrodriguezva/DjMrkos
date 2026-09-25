using DjMrkos.Application.Common.Exceptions;
using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Application.Events.Dtos;
using DjMrkos.Domain.Events;
using MediatR;

namespace DjMrkos.Application.Events.Queries;

/// <summary>
/// What the public QR landing page calls the moment a guest's phone opens it. Throws
/// <see cref="QrWindowClosedException"/> (HTTP 410) rather than a generic 404 so the
/// frontend can show "this event already ended" instead of "page not found".
/// </summary>
public sealed record GetEventByQrTokenQuery(string Token) : IRequest<EventPublicDto>;

public sealed class GetEventByQrTokenQueryHandler(IEventRepository repository, IDateTimeProvider clock)
    : IRequestHandler<GetEventByQrTokenQuery, EventPublicDto>
{
    public async Task<EventPublicDto> Handle(GetEventByQrTokenQuery request, CancellationToken cancellationToken)
    {
        var @event = await repository.GetByQrTokenAsync(request.Token, cancellationToken)
            ?? throw new NotFoundException(nameof(Event), request.Token);

        var isOpen = @event.IsQrWindowOpen(clock.UtcNow);
        return new EventPublicDto(@event.Id, @event.ClientName, @event.EventDateUtc, isOpen);
    }
}
