using DjMrkos.Domain.Testimonials;

namespace DjMrkos.Application.Testimonials.Dtos;

public sealed record TestimonialDto(Guid Id, string ClientName, Guid? EventId, int Rating, string Comment, bool IsApproved, DateTimeOffset CreatedAtUtc)
{
    public static TestimonialDto From(Testimonial entity) => new(
        entity.Id, entity.ClientName, entity.EventId, entity.Rating, entity.Comment, entity.IsApproved, entity.CreatedAtUtc);
}
