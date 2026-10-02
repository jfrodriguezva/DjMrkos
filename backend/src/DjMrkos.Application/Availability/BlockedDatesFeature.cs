using DjMrkos.Application.Common.Exceptions;
using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Domain.Availability;
using FluentValidation;
using MediatR;

namespace DjMrkos.Application.Availability;

public sealed record BlockedDateDto(Guid Id, DateOnly Date, string? Reason)
{
    public static BlockedDateDto From(BlockedDate entity) => new(entity.Id, entity.Date, entity.Reason);
}

/// <summary>Admin list: every blocked date from yesterday on (yesterday absorbs the UTC/Mexico offset).</summary>
public sealed record GetBlockedDatesQuery : IRequest<IReadOnlyList<BlockedDateDto>>;

public sealed class GetBlockedDatesQueryHandler(IBlockedDateRepository repository, IDateTimeProvider clock)
    : IRequestHandler<GetBlockedDatesQuery, IReadOnlyList<BlockedDateDto>>
{
    public async Task<IReadOnlyList<BlockedDateDto>> Handle(GetBlockedDatesQuery request, CancellationToken cancellationToken)
    {
        var from = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime).AddDays(-1);
        var blocked = await repository.GetFromAsync(from, cancellationToken);
        return blocked.Select(BlockedDateDto.From).ToList();
    }
}

/// <summary>Blocks one day (<c>To</c> omitted) or a whole range, e.g. a vacation. Days already blocked are skipped.</summary>
public sealed record BlockDatesCommand(DateOnly From, DateOnly? To, string? Reason) : IRequest<IReadOnlyList<BlockedDateDto>>;

public sealed class BlockDatesCommandValidator : AbstractValidator<BlockDatesCommand>
{
    public const int MaxDays = 90;

    public BlockDatesCommandValidator()
    {
        RuleFor(x => x.Reason).MaximumLength(200);
        RuleFor(x => x.To)
            .Must((cmd, to) => to is null || to.Value >= cmd.From)
            .WithMessage("La fecha final no puede ser anterior a la inicial.");
        RuleFor(x => x)
            .Must(x => x.To is null || x.To.Value.DayNumber - x.From.DayNumber < MaxDays)
            .WithMessage($"Bloquea como máximo {MaxDays} días a la vez.");
    }
}

public sealed class BlockDatesCommandHandler(IBlockedDateRepository repository)
    : IRequestHandler<BlockDatesCommand, IReadOnlyList<BlockedDateDto>>
{
    public async Task<IReadOnlyList<BlockedDateDto>> Handle(BlockDatesCommand request, CancellationToken cancellationToken)
    {
        var last = request.To ?? request.From;
        var dates = Enumerable.Range(0, last.DayNumber - request.From.DayNumber + 1)
            .Select(offset => request.From.AddDays(offset))
            .ToList();

        var existing = await repository.GetExistingAsync(dates, cancellationToken);
        var toAdd = dates
            .Where(d => !existing.Contains(d))
            .Select(d => BlockedDate.Create(d, request.Reason))
            .ToList();

        if (toAdd.Count > 0)
            await repository.AddRangeAsync(toAdd, cancellationToken);

        return toAdd.Select(BlockedDateDto.From).ToList();
    }
}

public sealed record UnblockDateCommand(Guid Id) : IRequest;

public sealed class UnblockDateCommandHandler(IBlockedDateRepository repository) : IRequestHandler<UnblockDateCommand>
{
    public async Task Handle(UnblockDateCommand request, CancellationToken cancellationToken)
    {
        _ = await repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(BlockedDate), request.Id);
        await repository.DeleteAsync(request.Id, cancellationToken);
    }
}
