using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Application.Testimonials.Dtos;
using MediatR;

namespace DjMrkos.Application.Testimonials.Queries;

/// <summary>Admin listing — includes the unapproved testimonials waiting for the DJ's review.</summary>
public sealed record GetAllTestimonialsQuery : IRequest<IReadOnlyList<TestimonialDto>>;

public sealed class GetAllTestimonialsQueryHandler(ITestimonialRepository repository)
    : IRequestHandler<GetAllTestimonialsQuery, IReadOnlyList<TestimonialDto>>
{
    public async Task<IReadOnlyList<TestimonialDto>> Handle(GetAllTestimonialsQuery request, CancellationToken cancellationToken)
    {
        var testimonials = await repository.GetAllAsync(cancellationToken);
        return testimonials.OrderByDescending(t => t.CreatedAtUtc).Select(TestimonialDto.From).ToList();
    }
}
