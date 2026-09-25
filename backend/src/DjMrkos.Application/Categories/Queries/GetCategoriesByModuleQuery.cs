using DjMrkos.Application.Categories.Dtos;
using DjMrkos.Application.Common.Interfaces;
using MediatR;

namespace DjMrkos.Application.Categories.Queries;

public sealed record GetCategoriesByModuleQuery(Guid ModuleId) : IRequest<IReadOnlyList<CategoryDto>>;

public sealed class GetCategoriesByModuleQueryHandler(ICategoryRepository repository)
    : IRequestHandler<GetCategoriesByModuleQuery, IReadOnlyList<CategoryDto>>
{
    public async Task<IReadOnlyList<CategoryDto>> Handle(GetCategoriesByModuleQuery request, CancellationToken cancellationToken)
    {
        var categories = await repository.GetByModuleIdAsync(request.ModuleId, onlyActive: false, cancellationToken);
        return categories.OrderBy(c => c.DisplayOrder).Select(CategoryDto.From).ToList();
    }
}
