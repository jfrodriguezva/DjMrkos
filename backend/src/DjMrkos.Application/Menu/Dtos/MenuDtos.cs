namespace DjMrkos.Application.Menu.Dtos;

/// <summary>
/// What the public header/nav renders — and, since it carries <see cref="Price"/>, also what
/// the quote builder's catalog is made of. One query, zero hardcoded menu items or prices.
/// <see cref="Price"/> is always the final, pay-this price (discounted if a promotion applies) —
/// it's what the cart adds up. <see cref="OriginalPrice"/> is only set when a promotion is active,
/// for the frontend to render as a struck-through "before" price next to the discount badge.
/// </summary>
public sealed record MenuCategoryDto(
    Guid Id, string Name, string Slug, string? Description, string? ImageUrl, decimal? Price,
    decimal? OriginalPrice, decimal? DiscountPercentage, string? PromotionLabel);

public sealed record MenuModuleDto(Guid Id, string Name, string Slug, string? Icon, IReadOnlyList<MenuCategoryDto> Categories);
