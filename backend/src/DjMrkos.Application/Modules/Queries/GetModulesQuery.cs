using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Application.Modules.Dtos;
using MediatR;

namespace DjMrkos.Application.Modules.Queries;

/// <summary>Admin listing — includes inactive modules, unlike the public menu query.</summary>
public sealed record GetModulesQuery : IRequest<IReadOnlyList<ModuleDto>>;

public sealed class GetModulesQueryHandler(IModuleRepository repository) : IRequestHandler<GetModulesQuery, IReadOnlyList<ModuleDto>>
{
    public async Task<IReadOnlyList<ModuleDto>> Handle(GetModulesQuery request, CancellationToken cancellationToken)
    {
        var all = await repository.GetAllAsync(onlyActive: false, cancellationToken);
        return all.OrderBy(m => m.DisplayOrder).Select(ModuleDto.From).ToList();
    }
}
