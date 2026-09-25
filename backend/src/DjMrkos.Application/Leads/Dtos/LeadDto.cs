using DjMrkos.Domain.Leads;

namespace DjMrkos.Application.Leads.Dtos;

public sealed record LeadDto(Guid Id, string Name, string Email, string? Phone, DateOnly? EventDate, string Message, LeadStatus Status, DateTimeOffset CreatedAtUtc)
{
    public static LeadDto From(Lead entity) => new(
        entity.Id, entity.Name, entity.Email, entity.Phone, entity.EventDate, entity.Message, entity.Status, entity.CreatedAtUtc);
}
