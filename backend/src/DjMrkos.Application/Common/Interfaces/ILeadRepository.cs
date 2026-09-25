using DjMrkos.Domain.Leads;

namespace DjMrkos.Application.Common.Interfaces;

public interface ILeadRepository
{
    Task<IReadOnlyList<Lead>> GetAllAsync(CancellationToken ct);
    Task<Lead> AddAsync(Lead lead, CancellationToken ct);
}
