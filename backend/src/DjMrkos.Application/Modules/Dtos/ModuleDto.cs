using DjMrkos.Domain.Modules;

namespace DjMrkos.Application.Modules.Dtos;

public sealed record ModuleDto(Guid Id, string Name, string Slug, string? Icon, int DisplayOrder, bool IsActive, DateTimeOffset CreatedAtUtc)
{
    public static ModuleDto From(Module entity) => new(
        entity.Id, entity.Name, entity.Slug, entity.Icon, entity.DisplayOrder, entity.IsActive, entity.CreatedAtUtc);
}
