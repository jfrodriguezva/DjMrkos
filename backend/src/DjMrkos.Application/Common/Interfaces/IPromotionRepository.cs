using DjMrkos.Domain.Promotions;

namespace DjMrkos.Application.Common.Interfaces;

public interface IPromotionRepository
{
    Task<Promotion?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Promotion>> GetAllAsync(CancellationToken ct);
    Task<IReadOnlyList<Promotion>> GetAllActiveAsync(CancellationToken ct);
    Task<Promotion> AddAsync(Promotion promotion, CancellationToken ct);
    Task UpdateAsync(Promotion promotion, CancellationToken ct);
}
