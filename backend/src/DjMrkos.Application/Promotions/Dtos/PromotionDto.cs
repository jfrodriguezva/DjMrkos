using DjMrkos.Domain.Promotions;

namespace DjMrkos.Application.Promotions.Dtos;

public sealed record PromotionDto(
    Guid Id, Guid? ModuleId, Guid? CategoryId, string Label, decimal DiscountPercentage, bool IsActive, DateTimeOffset CreatedAtUtc)
{
    public static PromotionDto From(Promotion entity) => new(
        entity.Id, entity.ModuleId, entity.CategoryId, entity.Label, entity.DiscountPercentage, entity.IsActive, entity.CreatedAtUtc);
}
