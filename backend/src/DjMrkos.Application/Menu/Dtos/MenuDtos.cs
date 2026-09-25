namespace DjMrkos.Application.Menu.Dtos;

/// <summary>What the public header/nav renders. One query, zero hardcoded menu items.</summary>
public sealed record MenuCategoryDto(Guid Id, string Name, string Slug, string? Description, string? ImageUrl);

public sealed record MenuModuleDto(Guid Id, string Name, string Slug, string? Icon, IReadOnlyList<MenuCategoryDto> Categories);
