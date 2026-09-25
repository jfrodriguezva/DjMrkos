using DjMrkos.Domain.Testimonials;

namespace DjMrkos.Application.Common.Interfaces;

public interface ITestimonialRepository
{
    Task<Testimonial?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Testimonial>> GetApprovedAsync(CancellationToken ct);
    Task<IReadOnlyList<Testimonial>> GetAllAsync(CancellationToken ct);
    Task<Testimonial> AddAsync(Testimonial testimonial, CancellationToken ct);
    Task UpdateAsync(Testimonial testimonial, CancellationToken ct);
}
