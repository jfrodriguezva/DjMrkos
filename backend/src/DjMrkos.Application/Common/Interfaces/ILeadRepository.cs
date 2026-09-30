using DjMrkos.Domain.Leads;

namespace DjMrkos.Application.Common.Interfaces;

public interface ILeadRepository
{
    Task<Lead?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Lead>> GetAllAsync(CancellationToken ct);
    Task<Lead> AddAsync(Lead lead, CancellationToken ct);
    Task UpdateAsync(Lead lead, CancellationToken ct);
}
