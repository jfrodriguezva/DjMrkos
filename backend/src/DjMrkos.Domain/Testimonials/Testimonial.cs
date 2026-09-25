using DjMrkos.Domain.Common;

namespace DjMrkos.Domain.Testimonials;

/// <summary>A client review. Never shown publicly until the DJ approves it.</summary>
public sealed class Testimonial : Entity
{
    public string ClientName { get; private set; } = string.Empty;
    public Guid? EventId { get; private set; }
    public int Rating { get; private set; }
    public string Comment { get; private set; } = string.Empty;
    public bool IsApproved { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private Testimonial() { }

    public static Testimonial Create(string clientName, Guid? eventId, int rating, string comment)
    {
        if (string.IsNullOrWhiteSpace(clientName))
            throw new DomainException("El testimonio necesita un nombre.");
        if (rating is < 1 or > 5)
            throw new DomainException("La calificación debe ser de 1 a 5.");
        if (string.IsNullOrWhiteSpace(comment))
            throw new DomainException("El testimonio necesita un comentario.");

        return new Testimonial
        {
            ClientName = clientName.Trim(),
            EventId = eventId,
            Rating = rating,
            Comment = comment.Trim(),
            IsApproved = false,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };
    }

    public static Testimonial Rehydrate(Guid id, string clientName, Guid? eventId, int rating, string comment, bool isApproved, DateTimeOffset createdAtUtc)
        => new()
        {
            Id = id,
            ClientName = clientName,
            EventId = eventId,
            Rating = rating,
            Comment = comment,
            IsApproved = isApproved,
            CreatedAtUtc = createdAtUtc,
        };

    public void Approve() => IsApproved = true;

    public void Reject() => IsApproved = false;
}
