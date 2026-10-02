using DjMrkos.Application.Availability;
using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Domain.Availability;
using NSubstitute;
using Xunit;

namespace DjMrkos.Application.UnitTests.Availability;

public sealed class BlockDatesCommandTests
{
    private readonly IBlockedDateRepository _repository = Substitute.For<IBlockedDateRepository>();
    private readonly BlockDatesCommandHandler _sut;

    public BlockDatesCommandTests() => _sut = new BlockDatesCommandHandler(_repository);

    [Fact]
    public async Task Handle_Range_BlocksEveryDayAndSkipsTheOnesAlreadyBlocked()
    {
        var from = new DateOnly(2026, 12, 24);
        _repository.GetExistingAsync(Arg.Any<IReadOnlyCollection<DateOnly>>(), Arg.Any<CancellationToken>())
            .Returns(new HashSet<DateOnly> { new(2026, 12, 25) });

        var result = await _sut.Handle(new BlockDatesCommand(from, new DateOnly(2026, 12, 27), "Vacaciones"), CancellationToken.None);

        Assert.Equal([new DateOnly(2026, 12, 24), new DateOnly(2026, 12, 26), new DateOnly(2026, 12, 27)], result.Select(r => r.Date));
        Assert.All(result, r => Assert.Equal("Vacaciones", r.Reason));
        await _repository.Received(1).AddRangeAsync(Arg.Is<IReadOnlyCollection<BlockedDate>>(b => b.Count == 3), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEveryDayIsAlreadyBlocked_WritesNothing()
    {
        var day = new DateOnly(2026, 12, 31);
        _repository.GetExistingAsync(Arg.Any<IReadOnlyCollection<DateOnly>>(), Arg.Any<CancellationToken>())
            .Returns(new HashSet<DateOnly> { day });

        var result = await _sut.Handle(new BlockDatesCommand(day, null, null), CancellationToken.None);

        Assert.Empty(result);
        await _repository.DidNotReceiveWithAnyArgs().AddRangeAsync(default!, default);
    }

    [Theory]
    [InlineData(-1, false)] // end before start
    [InlineData(89, true)]  // 90 days inclusive
    [InlineData(90, false)] // 91 days
    public void Validator_EnforcesOrderAndMaximumRange(int extraDays, bool valid)
    {
        var from = new DateOnly(2027, 1, 1);
        var result = new BlockDatesCommandValidator().Validate(new BlockDatesCommand(from, from.AddDays(extraDays), null));

        Assert.Equal(valid, result.IsValid);
    }
}
