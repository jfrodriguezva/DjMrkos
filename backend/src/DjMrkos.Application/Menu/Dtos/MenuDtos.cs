namespace DjMrkos.Application.Menu.Dtos;

/// <summary>
/// What the public header/nav renders — and, since it carries <see cref="Price"/>, also what
/// the quote builder's catalog is made of. One query, zero hardcoded menu items or prices.
/// </summary>
public sealed record MenuCategoryDto(Guid Id, string Name, string Slug, string? Description, string? ImageUrl, decimal? Price);

public sealed record MenuModuleDto(Guid Id, string Name, string Slug, string? Icon, IReadOnlyList<MenuCategoryDto> Categories);
