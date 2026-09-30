using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Application.Promotions.Dtos;
using MediatR;

namespace DjMrkos.Application.Promotions.Queries;

public sealed record GetPromotionsQuery : IRequest<IReadOnlyList<PromotionDto>>;

public sealed class GetPromotionsQueryHandler(IPromotionRepository promotions) : IRequestHandler<GetPromotionsQuery, IReadOnlyList<PromotionDto>>
{
    public async Task<IReadOnlyList<PromotionDto>> Handle(GetPromotionsQuery request, CancellationToken cancellationToken)
    {
        var all = await promotions.GetAllAsync(cancellationToken);
        return all.OrderByDescending(p => p.CreatedAtUtc).Select(PromotionDto.From).ToList();
    }
}
