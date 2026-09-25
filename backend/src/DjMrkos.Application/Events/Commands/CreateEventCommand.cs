using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Application.Events.Dtos;
using DjMrkos.Domain.Events;
using FluentValidation;
using MediatR;

namespace DjMrkos.Application.Events.Commands;

public sealed record CreateEventCommand(string ClientName, string? Location, DateTimeOffset EventDateUtc) : IRequest<EventDto>;

public sealed class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.ClientName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.EventDateUtc).NotEmpty();
    }
}

/// <summary>
/// Scheduling an event is what mints its QR token — the token only starts existing once
/// there's a real event for it to unlock, which is what makes the whole flow tamper-proof.
/// </summary>
public sealed class CreateEventCommandHandler(IEventRepository repository, IQrTokenService qrTokenService)
    : IRequestHandler<CreateEventCommand, EventDto>
{
    public async Task<EventDto> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var scheduled = Event.Schedule(request.ClientName, request.Location, request.EventDateUtc);
        scheduled.IssueQrToken(qrTokenService.IssueToken(scheduled.Id));

        var created = await repository.AddAsync(scheduled, cancellationToken);
        return EventDto.From(created);
    }
}
