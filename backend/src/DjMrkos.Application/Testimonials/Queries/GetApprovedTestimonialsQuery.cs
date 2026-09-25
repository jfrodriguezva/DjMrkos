using DjMrkos.Application.Common.Interfaces;
using DjMrkos.Application.Testimonials.Dtos;
using MediatR;

namespace DjMrkos.Application.Testimonials.Queries;

public sealed record GetApprovedTestimonialsQuery : IRequest<IReadOnlyList<TestimonialDto>>;

public sealed class GetApprovedTestimonialsQueryHandler(ITestimonialRepository repository)
    : IRequestHandler<GetApprovedTestimonialsQuery, IReadOnlyList<TestimonialDto>>
{
    public async Task<IReadOnlyList<TestimonialDto>> Handle(GetApprovedTestimonialsQuery request, CancellationToken cancellationToken)
    {
        var testimonials = await repository.GetApprovedAsync(cancellationToken);
        return testimonials.OrderByDescending(t => t.CreatedAtUtc).Select(TestimonialDto.From).ToList();
    }
}
