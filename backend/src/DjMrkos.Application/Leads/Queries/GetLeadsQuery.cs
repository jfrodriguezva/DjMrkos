using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Application.Leads.Dtos;
using MediatR;

namespace DjMrkos.Application.Leads.Queries;

public sealed record GetLeadsQuery : IRequest<IReadOnlyList<LeadDto>>;

public sealed class GetLeadsQueryHandler(ILeadRepository repository) : IRequestHandler<GetLeadsQuery, IReadOnlyList<LeadDto>>
{
    public async Task<IReadOnlyList<LeadDto>> Handle(GetLeadsQuery request, CancellationToken cancellationToken)
    {
        var leads = await repository.GetAllAsync(cancellationToken);
        return leads.OrderByDescending(l => l.CreatedAtUtc).Select(LeadDto.From).ToList();
    }
}
