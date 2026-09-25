using DjMrkos.Application.Common.Exceptions;
using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Application.SongRequests.Commands;
using DjMrkos.Domain.Events;
using DjMrkos.Domain.SongRequests;
using NSubstitute;
using Xunit;

namespace DjMrkos.Application.UnitTests.SongRequests;

/// <summary>
/// The QR song-request flow is the product's centerpiece, so its guard rails — the QR
/// window and the per-device rate limit — get their own tests instead of only being
/// exercised indirectly through the API.
/// </summary>
public sealed class CreateSongRequestCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 1, 22, 0, 0, TimeSpan.Zero);

    private readonly IEventRepository _events = Substitute.For<IEventRepository>();
    private readonly ISongRequestRepository _songRequests = Substitute.For<ISongRequestRepository>();
    private readonly ISongRequestNotifier _notifier = Substitute.For<ISongRequestNotifier>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly CreateSongRequestCommandHandler _sut;

    public CreateSongRequestCommandHandlerTests()
    {
        _clock.UtcNow.Returns(Now);
        _sut = new CreateSongRequestCommandHandler(_events, _songRequests, _notifier, _clock);
    }

    [Fact]
    public async Task Handle_WhenEventDoesNotExist_ThrowsNotFound()
    {
        _events.GetByQrTokenAsync("missing-token", Arg.Any<CancellationToken>()).Returns((Event?)null);

        var command = new CreateSongRequestCommand("missing-token", "Song", null, null, null, "fingerprint-1");

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenQrWindowIsClosed_ThrowsQrWindowClosed()
    {
        var pastEvent = Event.Schedule("Boda de Ana y Luis", "Salón Jardín", Now.AddDays(-3));
        pastEvent.IssueQrToken("closed-token");
        _events.GetByQrTokenAsync("closed-token", Arg.Any<CancellationToken>()).Returns(pastEvent);

        var command = new CreateSongRequestCommand("closed-token", "Song", null, null, null, "fingerprint-1");

        await Assert.ThrowsAsync<QrWindowClosedException>(() => _sut.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenFingerprintExceedsRateLimit_ThrowsRateLimitExceeded()
    {
        var liveEvent = Event.Schedule("XV de Camila", "Salón Real", Now);
        liveEvent.IssueQrToken("live-token");
        _events.GetByQrTokenAsync("live-token", Arg.Any<CancellationToken>()).Returns(liveEvent);
        _songRequests.CountByFingerprintSinceAsync(liveEvent.Id, "fingerprint-1", Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(5);

        var command = new CreateSongRequestCommand("live-token", "Song", null, null, null, "fingerprint-1");

        await Assert.ThrowsAsync<RateLimitExceededException>(() => _sut.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithValidWindowAndFreshFingerprint_CreatesRequestAndNotifies()
    {
        var liveEvent = Event.Schedule("Aniversario Hernández", "Terraza Norte", Now);
        liveEvent.IssueQrToken("live-token");
        _events.GetByQrTokenAsync("live-token", Arg.Any<CancellationToken>()).Returns(liveEvent);
        _songRequests.CountByFingerprintSinceAsync(liveEvent.Id, "fingerprint-1", Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(0);
        _songRequests.AddAsync(Arg.Any<SongRequest>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<SongRequest>());

        var command = new CreateSongRequestCommand("live-token", "La Bikina", "Luis Miguel", "Para mi abuela", "¡Salud!", "fingerprint-1");

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.Equal("La Bikina", result.SongTitle);
        Assert.Equal(SongRequestStatus.Pending, result.Status);
        await _notifier.Received(1).NotifyRequestCreatedAsync(liveEvent.Id, Arg.Any<Application.SongRequests.Dtos.SongRequestDto>(), Arg.Any<CancellationToken>());
    }
}
