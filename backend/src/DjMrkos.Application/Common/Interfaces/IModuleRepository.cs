using DjMrkos.Domain.Modules;

namespace DjMrkos.Application.Common.Interfaces;

public interface IModuleRepository
{
    Task<Module?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Module>> GetAllAsync(bool onlyActive, CancellationToken ct);
    Task<Module> AddAsync(Module module, CancellationToken ct);
    Task UpdateAsync(Module module, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}
