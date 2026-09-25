using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Application.SongRequests.Dtos;
using MediatR;

namespace DjMrkos.Application.SongRequests.Queries;

/// <summary>Feeds the DJ's live dashboard on first load; SignalR takes over from there.</summary>
public sealed record GetSongRequestsByEventQuery(Guid EventId) : IRequest<IReadOnlyList<SongRequestDto>>;

public sealed class GetSongRequestsByEventQueryHandler(ISongRequestRepository repository)
    : IRequestHandler<GetSongRequestsByEventQuery, IReadOnlyList<SongRequestDto>>
{
    public async Task<IReadOnlyList<SongRequestDto>> Handle(GetSongRequestsByEventQuery request, CancellationToken cancellationToken)
    {
        var requests = await repository.GetByEventIdAsync(request.EventId, cancellationToken);
        return requests.OrderByDescending(r => r.CreatedAtUtc).Select(SongRequestDto.From).ToList();
    }
}
