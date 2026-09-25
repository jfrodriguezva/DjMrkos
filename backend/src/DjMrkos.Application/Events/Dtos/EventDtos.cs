using DjMrkos.Domain.Events;

namespace DjMrkos.Application.Events.Dtos;

/// <summary>Full record, for the admin panel.</summary>
public sealed record EventDto(
    Guid Id, string ClientName, string? Location, DateTimeOffset EventDateUtc, EventStatus Status,
    string QrToken, DateTimeOffset QrValidFromUtc, DateTimeOffset QrValidUntilUtc)
{
    public static EventDto From(Event entity) => new(
        entity.Id, entity.ClientName, entity.Location, entity.EventDateUtc, entity.Status,
        entity.QrToken, entity.QrValidFromUtc, entity.QrValidUntilUtc);
}

/// <summary>Safe subset shown to a guest who just scanned the QR — no admin data leaks here.</summary>
public sealed record EventPublicDto(Guid Id, string ClientName, DateTimeOffset EventDateUtc, bool IsRequestWindowOpen);
