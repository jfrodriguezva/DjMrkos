using DjMrkos.Domain.SongRequests;

namespace DjMrkos.Application.SongRequests.Dtos;

public sealed record SongRequestDto(
    Guid Id,
    Guid EventId,
    string SongTitle,
    string? Artist,
    string? RequesterName,
    string? Dedication,
    SongRequestStatus Status,
    DateTimeOffset CreatedAtUtc)
{
    public static SongRequestDto From(SongRequest entity) => new(
        entity.Id, entity.EventId, entity.SongTitle, entity.Artist,
        entity.RequesterName, entity.Dedication, entity.Status, entity.CreatedAtUtc);
}
