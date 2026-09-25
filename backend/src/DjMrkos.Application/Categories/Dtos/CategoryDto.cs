using DjMrkos.Domain.Modules;

namespace DjMrkos.Application.Categories.Dtos;

public sealed record CategoryDto(
    Guid Id, Guid ModuleId, string Name, string Slug, string? Description,
    string? ImageUrl, int DisplayOrder, bool IsActive, DateTimeOffset CreatedAtUtc)
{
    public static CategoryDto From(Category entity) => new(
        entity.Id, entity.ModuleId, entity.Name, entity.Slug, entity.Description,
        entity.ImageUrl, entity.DisplayOrder, entity.IsActive, entity.CreatedAtUtc);
}
