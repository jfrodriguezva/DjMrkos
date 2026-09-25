using DjMrkos.Domain.Common;

namespace DjMrkos.Domain.Leads;

/// <summary>A quote request submitted from the public contact form.</summary>
public sealed class Lead : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public DateOnly? EventDate { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public LeadStatus Status { get; private set; } = LeadStatus.New;
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private Lead() { }

    public static Lead Create(string name, string email, string? phone, DateOnly? eventDate, string message)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre es obligatorio.");
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("El correo es obligatorio.");

        return new Lead
        {
            Name = name.Trim(),
            Email = email.Trim(),
            Phone = phone,
            EventDate = eventDate,
            Message = message.Trim(),
            Status = LeadStatus.New,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };
    }

    public static Lead Rehydrate(Guid id, string name, string email, string? phone, DateOnly? eventDate, string message, LeadStatus status, DateTimeOffset createdAtUtc)
        => new()
        {
            Id = id,
            Name = name,
            Email = email,
            Phone = phone,
            EventDate = eventDate,
            Message = message,
            Status = status,
            CreatedAtUtc = createdAtUtc,
        };

    public void MarkContacted() => Status = LeadStatus.Contacted;

    public void MarkWon() => Status = LeadStatus.Won;

    public void MarkLost() => Status = LeadStatus.Lost;
}
