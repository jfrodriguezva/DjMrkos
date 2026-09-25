using DjMrkos.Domain.Modules;

namespace DjMrkos.Application.Common.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Category>> GetByModuleIdAsync(Guid moduleId, bool onlyActive, CancellationToken ct);
    Task<IReadOnlyList<Category>> GetAllActiveAsync(CancellationToken ct);
    Task<Category> AddAsync(Category category, CancellationToken ct);
    Task UpdateAsync(Category category, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}
