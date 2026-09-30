using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Application.Menu.Dtos;
using DjMrkos.Domain.Modules;
using DjMrkos.Domain.Promotions;
using MediatR;

namespace DjMrkos.Application.Menu.Queries;

/// <summary>
/// The entire public navigation, in one call: active modules in display order, each with
/// its active categories in display order. Add a module in the admin panel and it shows up
/// here on the next request — no deploy, no code change.
/// </summary>
public sealed record GetPublicMenuQuery : IRequest<IReadOnlyList<MenuModuleDto>>;

public sealed class GetPublicMenuQueryHandler(IModuleRepository modules, ICategoryRepository categories, IPromotionRepository promotions)
    : IRequestHandler<GetPublicMenuQuery, IReadOnlyList<MenuModuleDto>>
{
    public async Task<IReadOnlyList<MenuModuleDto>> Handle(GetPublicMenuQuery request, CancellationToken cancellationToken)
    {
        var activeModules = await modules.GetAllAsync(onlyActive: true, cancellationToken);
        var activeCategories = await categories.GetAllActiveAsync(cancellationToken);
        var activePromotions = await promotions.GetAllActiveAsync(cancellationToken);
        var categoriesByModule = activeCategories.ToLookup(c => c.ModuleId);

        // A category-level promotion takes priority over a module-wide one for the same category.
        var promotionByCategoryId = activePromotions.Where(p => p.CategoryId is not null).ToDictionary(p => p.CategoryId!.Value);
        var promotionByModuleId = activePromotions.Where(p => p.ModuleId is not null).ToDictionary(p => p.ModuleId!.Value);

        return activeModules
            .OrderBy(m => m.DisplayOrder)
            .Select(m => new MenuModuleDto(
                m.Id,
                m.Name,
                m.Slug,
                m.Icon,
                categoriesByModule[m.Id]
                    .OrderBy(c => c.DisplayOrder)
                    .Select(c => ToDto(c, promotionByCategoryId.GetValueOrDefault(c.Id) ?? promotionByModuleId.GetValueOrDefault(m.Id)))
                    .ToList()))
            .ToList();
    }

    private static MenuCategoryDto ToDto(Category c, Promotion? promotion)
    {
        if (promotion is null || c.Price is null)
            return new MenuCategoryDto(c.Id, c.Name, c.Slug, c.Description, c.ImageUrl, c.Price, null, null, null);

        var discounted = Math.Round(c.Price.Value * (1 - promotion.DiscountPercentage / 100m), 2);
        return new MenuCategoryDto(c.Id, c.Name, c.Slug, c.Description, c.ImageUrl, discounted, c.Price, promotion.DiscountPercentage, promotion.Label);
    }
}
