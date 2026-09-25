using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Application.Menu.Dtos;
using MediatR;

namespace DjMrkos.Application.Menu.Queries;

/// <summary>
/// The entire public navigation, in one call: active modules in display order, each with
/// its active categories in display order. Add a module in the admin panel and it shows up
/// here on the next request — no deploy, no code change.
/// </summary>
public sealed record GetPublicMenuQuery : IRequest<IReadOnlyList<MenuModuleDto>>;

public sealed class GetPublicMenuQueryHandler(IModuleRepository modules, ICategoryRepository categories)
    : IRequestHandler<GetPublicMenuQuery, IReadOnlyList<MenuModuleDto>>
{
    public async Task<IReadOnlyList<MenuModuleDto>> Handle(GetPublicMenuQuery request, CancellationToken cancellationToken)
    {
        var activeModules = await modules.GetAllAsync(onlyActive: true, cancellationToken);
        var activeCategories = await categories.GetAllActiveAsync(cancellationToken);
        var categoriesByModule = activeCategories.ToLookup(c => c.ModuleId);

        return activeModules
            .OrderBy(m => m.DisplayOrder)
            .Select(m => new MenuModuleDto(
                m.Id,
                m.Name,
                m.Slug,
                m.Icon,
                categoriesByModule[m.Id]
                    .OrderBy(c => c.DisplayOrder)
                    .Select(c => new MenuCategoryDto(c.Id, c.Name, c.Slug, c.Description, c.ImageUrl, c.Price))
                    .ToList()))
            .ToList();
    }
}
