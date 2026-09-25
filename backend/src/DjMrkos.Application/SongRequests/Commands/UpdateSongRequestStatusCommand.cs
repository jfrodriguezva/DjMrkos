using DjMrkos.Application.Common.Exceptions;
using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Application.SongRequests.Dtos;
using DjMrkos.Domain.SongRequests;
using FluentValidation;
using MediatR;

namespace DjMrkos.Application.SongRequests.Commands;

public enum SongRequestAction { Queue, MarkPlayed, Reject }

public sealed record UpdateSongRequestStatusCommand(Guid Id, SongRequestAction Action) : IRequest<SongRequestDto>;

public sealed class UpdateSongRequestStatusCommandValidator : AbstractValidator<UpdateSongRequestStatusCommand>
{
    public UpdateSongRequestStatusCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

/// <summary>What the DJ's dashboard calls when they tap a request in the live queue.</summary>
public sealed class UpdateSongRequestStatusCommandHandler(ISongRequestRepository repository, ISongRequestNotifier notifier)
    : IRequestHandler<UpdateSongRequestStatusCommand, SongRequestDto>
{
    public async Task<SongRequestDto> Handle(UpdateSongRequestStatusCommand request, CancellationToken cancellationToken)
    {
        var songRequest = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(SongRequest), request.Id);

        switch (request.Action)
        {
            case SongRequestAction.Queue: songRequest.MoveToQueue(); break;
            case SongRequestAction.MarkPlayed: songRequest.MarkPlayed(); break;
            case SongRequestAction.Reject: songRequest.Reject(); break;
        }

        await repository.UpdateAsync(songRequest, cancellationToken);
        var dto = SongRequestDto.From(songRequest);

        await notifier.NotifyRequestUpdatedAsync(songRequest.EventId, dto, cancellationToken);

        return dto;
    }
}
