using DjMrkos.Application.Common.Exceptions;
using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Application.SongRequests.Dtos;
using DjMrkos.Domain.Events;
using DjMrkos.Domain.SongRequests;
using FluentValidation;
using MediatR;

namespace DjMrkos.Application.SongRequests.Commands;

/// <param name="RequesterFingerprint">Hash of the guest's device, supplied by the API layer — never trust a client-sent id alone.</param>
public sealed record CreateSongRequestCommand(
    string QrToken, string SongTitle, string? Artist, string? RequesterName, string? Dedication, string RequesterFingerprint)
    : IRequest<SongRequestDto>;

public sealed class CreateSongRequestCommandValidator : AbstractValidator<CreateSongRequestCommand>
{
    public CreateSongRequestCommandValidator()
    {
        RuleFor(x => x.QrToken).NotEmpty();
        RuleFor(x => x.SongTitle).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Artist).MaximumLength(120);
        RuleFor(x => x.RequesterName).MaximumLength(80);
        RuleFor(x => x.Dedication).MaximumLength(300);
        RuleFor(x => x.RequesterFingerprint).NotEmpty();
    }
}

public sealed class CreateSongRequestCommandHandler(
    IEventRepository events,
    ISongRequestRepository songRequests,
    ISongRequestNotifier notifier,
    IDjAlertNotifier djAlerts,
    IDateTimeProvider clock)
    : IRequestHandler<CreateSongRequestCommand, SongRequestDto>
{
    /// <summary>Same device, same event, more than this many requests in the window below → 429.</summary>
    private const int MaxRequestsPerWindow = 5;
    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(10);

    public async Task<SongRequestDto> Handle(CreateSongRequestCommand request, CancellationToken cancellationToken)
    {
        var @event = await events.GetByQrTokenAsync(request.QrToken, cancellationToken)
            ?? throw new NotFoundException(nameof(Event), request.QrToken);

        if (!@event.IsQrWindowOpen(clock.UtcNow))
            throw new QrWindowClosedException(@event.Id);

        var recentCount = await songRequests.CountByFingerprintSinceAsync(
            @event.Id, request.RequesterFingerprint, clock.UtcNow - RateLimitWindow, cancellationToken);

        if (recentCount >= MaxRequestsPerWindow)
            throw new RateLimitExceededException("Ya pediste varias canciones — espera un momento antes de pedir otra.");

        var songRequest = SongRequest.Create(
            @event.Id, request.SongTitle, request.Artist, request.RequesterName, request.Dedication, request.RequesterFingerprint);

        var created = await songRequests.AddAsync(songRequest, cancellationToken);
        var dto = SongRequestDto.From(created);

        await notifier.NotifyRequestCreatedAsync(@event.Id, dto, cancellationToken);
        await djAlerts.NotifyNewSongRequestAsync(request.SongTitle, request.Artist, request.RequesterName, cancellationToken);

        return dto;
    }
}
